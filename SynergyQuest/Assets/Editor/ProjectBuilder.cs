// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using Editor.Validation;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/**
 * <summary>
 * Adds menu actions to the Unity Editor which allow to build and bundle the whole game project, including the web app(s).
 * Works for linux and windows.
 * </summary>
 * <remarks>
 * Requirement: yarn (https://yarnpkg.com/) is installed on the system PATH
 * </remarks>
 */
public class ProjectBuilder
{
    // === Paths and other constants needed for building ===
    
    // ReSharper disable once PossibleNullReferenceException
    public static readonly Lazy<string> ProjectDir = new Lazy<string>(() => Directory.GetParent(Application.dataPath).Parent.FullName);
    
    private static readonly Lazy<string> GameProjectDir = new Lazy<string>(() => Directory.GetParent(Application.dataPath).FullName);
    
    private static readonly Lazy<string> ControllerProjectsDir = new Lazy<string>(() => Path.Combine(ProjectDir.Value, "Controller"));
    private static readonly Lazy<string> BuildScriptsProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "build-scripts"));
    private static readonly Lazy<string> ControllerClientLibProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "controller-client-lib"));
    private static readonly Lazy<string> SensorInputLibProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "sensor-input-lib"));
    private static readonly Lazy<string> ControllerAppProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "controller-app"));
    private static readonly Lazy<string> SslWarningInfoAppProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "ssl-warning-info"));
    // We expect web components to generate a list of dependencies and a disclaimer file at these locations:
    private static readonly string WebLibListBuildPath = "build/dependencies-list.txt";
    private static readonly string WebLibDisclaimerBuildPath = "build/dependencies-disclaimers.txt";
    
    private static readonly Lazy<string> BuildDirectory = new Lazy<string>(() => Path.Combine(GameProjectDir.Value, "Build"));
    private static readonly Lazy<string> WindowsBuildDirectory = new Lazy<string>(() => Path.Combine(Path.Combine(BuildDirectory.Value, "Windows"), "SynergyQuest"));
    private static readonly Lazy<string> LinuxBuildDirectory = new Lazy<string>(() => Path.Combine(Path.Combine(BuildDirectory.Value, "Linux.x86_64"), "SynergyQuest"));
    
    private static readonly Lazy<string> ReadmeFile = new Lazy<string>(() => Path.Combine(Path.Combine(GameProjectDir.Value, "UsageNotes"), "ReadMe.txt"));

    private const string WindowsExecutable = "SynergyQuest.exe";
    private const string LinuxExecutable = "SynergyQuest.x86_64";
    
    private static string[] ScenesToBuild => EditorBuildSettings.scenes
        .Where(scene => scene.enabled)
        .Select(scene => scene.path)
        .ToArray();


    // === Menu Actions ===
    
    /**
     * <summary>
     * Removes the build files for all sub-projects from the file system (game, web libraries, web apps, ...)
     * </summary>
     */
    [MenuItem("SynergyQuest Tools/Build/Clean Builds")]
    public static void CleanBuilds()
    {
        FileUtil.DeleteFileOrDirectory(BuildDirectory.Value.WinToNixPath());
        
        FileUtil.DeleteFileOrDirectory(Path.Combine(BuildScriptsProjectDir.Value, "dist").WinToNixPath());
        
        FileUtil.DeleteFileOrDirectory(Path.Combine(ControllerClientLibProjectDir.Value, "dist").WinToNixPath());
        FileUtil.DeleteFileOrDirectory(Path.Combine(ControllerClientLibProjectDir.Value, "node_modules").WinToNixPath());
        
        FileUtil.DeleteFileOrDirectory(Path.Combine(SensorInputLibProjectDir.Value, "dist").WinToNixPath());
        FileUtil.DeleteFileOrDirectory(Path.Combine(SensorInputLibProjectDir.Value, "node_modules").WinToNixPath());
        
        FileUtil.DeleteFileOrDirectory(Path.Combine(ControllerAppProjectDir.Value, "build").WinToNixPath());
        FileUtil.DeleteFileOrDirectory(Path.Combine(ControllerAppProjectDir.Value, "node_modules").WinToNixPath());
        
        FileUtil.DeleteFileOrDirectory(Path.Combine(SslWarningInfoAppProjectDir.Value, "build").WinToNixPath());
        FileUtil.DeleteFileOrDirectory(Path.Combine(SslWarningInfoAppProjectDir.Value, "node_modules").WinToNixPath());
        
        Log("Cleared all build folders.");
    }

    /**
     * <summary>
     * Builds and bundles a standalone version of SynergyQuest for Windows
     * (Including all web libraries and apps etc.)
     * </summary>
     */
    [MenuItem("SynergyQuest Tools/Build/Windows Build")]
    public static void BuildForWindowsBuildWeb()
    {
        BuildForWindows(false);
    }
    
    /**
     * <summary>
     * Builds and bundles a standalone version of SynergyQuest for Windows.
     * Assumes, all web components are already pre-built.
     * </summary>
     */
    public static void BuildForWindowsBuildNoWeb()
    {
        BuildForWindows(true);
    }
    
    /**
     * <summary>
     * Builds and bundles a standalone version of SynergyQuest for Windows
     * (Including all web libraries and apps etc.)
     * </summary>
     * <param name="assumePrebuiltWebComponents">Iff false, also builds web libraries and apps with yarn.</param>
     */
    public static void BuildForWindows(bool assumePrebuiltWebComponents)
    {
        CrossPlatformBuildPreamble(assumePrebuiltWebComponents);
        
        BuildWindowsPlayer();
        BuildWebComponents(WindowsBuildDirectory.Value, assumePrebuiltWebComponents);
        CopyAdditionalFiles(WindowsBuildDirectory.Value);
        
        DisplayBuildFinishedMessage("Windows", WindowsBuildDirectory.Value);
    }
    
    /**
     * <summary>
     * Builds and bundles a standalone version of SynergyQuest for Linux.
     * (Including all web libraries and apps etc.)
     * </summary>
     */
    [MenuItem("SynergyQuest Tools/Build/Linux Build")]
    public static void BuildForLinuxBuildWeb()
    {
        BuildForLinux(false);
    }

    /**
     * <summary>
     * Builds and bundles a standalone version of SynergyQuest for Linux.
     * Assumes, all web components are already pre-built.
     * </summary>
     */
    public static void BuildForLinuxBuildNoWeb()
    {
        BuildForLinux(true);
    }
    
    /**
     * <summary>
     * Builds and bundles a standalone version of SynergyQuest for Linux
     * (Including all web libraries and apps etc.)
     * </summary>
     * <param name="assumePrebuiltWebComponents">Iff false, also builds web libraries and apps with yarn.</param>
     */
    public static void BuildForLinux(bool assumePrebuiltWebComponents)
    {
        CrossPlatformBuildPreamble(assumePrebuiltWebComponents);
        
        BuildLinuxPlayer();
        BuildWebComponents(LinuxBuildDirectory.Value, assumePrebuiltWebComponents);
        CopyAdditionalFiles(LinuxBuildDirectory.Value);

        DisplayBuildFinishedMessage("Linux", LinuxBuildDirectory.Value);
    }
    
    private static void CrossPlatformBuildPreamble(bool assumePrebuiltWebComponents)
    {
        RunValidators();
        
        if (!assumePrebuiltWebComponents)
        {
            // Force search for Yarn
            YarnUtils.EnsureYarnIsInstalled();
        }
    }

    // === Helper methods for building sub-components ===
    private static void BuildLinuxPlayer()
    {
        Log("=== Building standalone Linux Player ===");
        BuildPipeline.BuildPlayer(
            new BuildPlayerOptions
            {
                locationPathName = Path.Combine(LinuxBuildDirectory.Value, LinuxExecutable).WinToNixPath(),
                scenes = ScenesToBuild,
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            }
        );
    }

    private static void BuildWindowsPlayer()
    {
        Log("=== Building standalone Windows Player ===");
        BuildPipeline.BuildPlayer(
            new BuildPlayerOptions
            {
                locationPathName = Path.Combine(WindowsBuildDirectory.Value, WindowsExecutable).WinToNixPath(),
                scenes = ScenesToBuild,
                target = BuildTarget.StandaloneWindows,
                options = BuildOptions.None
            }
        );
    }

    private static void BuildWebComponents(string buildDirectory, bool assumePrebuiltWebComponents)
    {
        try
        {
            InitializeLibsDisclaimer(buildDirectory);

            if (!assumePrebuiltWebComponents)
            {
                BuildBuildScripts();
            }
            
            if (!assumePrebuiltWebComponents)
            {
                BuildControllerApp();
            }
            CopyControllerApp(buildDirectory);
            
            if (!assumePrebuiltWebComponents)
            {
                BuildSslWarningInfoApp();
            }
            CopySslWarningInfoApp(buildDirectory);
        }
            
        catch
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }

    private static void CopyAdditionalFiles(string buildDirectory)
    {
        try
        {
            CopyCertificateFiles(buildDirectory);
            
            CopyReadme(buildDirectory);
        }
            
        catch
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
    }

    private static void DisplayBuildFinishedMessage(string target, string buildDirectory)
    {
        var title = $"=== {target} build finished ===";
        var msg = $"The build can be found at {buildDirectory}.";
        
        Log(title);
        Log(msg);
        EditorUtility.DisplayDialog(
            title,
            msg,
            "Ok"
        );
    }
    
    private static void BuildBuildScripts()
    {
        var title = "=== Building build-scripts with yarn ===";
        Log(title);
        
        EditorUtility.DisplayProgressBar(title, "building...", 0.0f);
        YarnUtils.Install(BuildScriptsProjectDir.Value);
        YarnUtils.Build(BuildScriptsProjectDir.Value);
        EditorUtility.ClearProgressBar();

        Log("Done: build-scripts");
    }
    
    private static void BuildControllerClientLib()
    {
        var title = "=== Building controller-client-lib with yarn ===";
        Log(title);
        
        EditorUtility.DisplayProgressBar(title, "installing dependencies...", 0.0f);
        YarnUtils.Install(ControllerClientLibProjectDir.Value);
        EditorUtility.DisplayProgressBar(title, "building...", 0.5f);
        YarnUtils.PrepareDistribution(ControllerClientLibProjectDir.Value);
        EditorUtility.ClearProgressBar();

        Log("Done: controller-client-lib");
    }
    
    private static void BuildSensorInputLib()
    {
        var title = "=== Building sensor-input-lib with yarn ===";
        Log(title);
        
        EditorUtility.DisplayProgressBar(title, "installing dependencies...", 0.0f);
        YarnUtils.Install(SensorInputLibProjectDir.Value);
        EditorUtility.DisplayProgressBar(title, "building...", 0.5f);
        YarnUtils.PrepareDistribution(SensorInputLibProjectDir.Value);
        EditorUtility.ClearProgressBar();
        
        Log("Done: sensor-input-lib");
    }
    
    private static void BuildControllerApp()
    {
        BuildControllerClientLib();
        BuildSensorInputLib();
        
        var title = "=== Building main controller web app with yarn ===";
        Log(title);

        EditorUtility.DisplayProgressBar(title, "installing dependencies...", 0.0f);
        YarnUtils.Install(ControllerAppProjectDir.Value);
        EditorUtility.DisplayProgressBar(title, "building...", 0.5f);
        YarnUtils.PrepareDistribution(ControllerAppProjectDir.Value);
        EditorUtility.ClearProgressBar();
        
        Log("Done: controller-app");
    }
    
    private static void BuildSslWarningInfoApp()
    {
        var title = "=== Building ssl-warning-info web app with yarn ===";
        Log(title);

        EditorUtility.DisplayProgressBar(title, "installing dependencies...", 0.0f);
        YarnUtils.Install(SslWarningInfoAppProjectDir.Value);
        EditorUtility.DisplayProgressBar(title, "building...", 0.5f);
        YarnUtils.PrepareDistribution(SslWarningInfoAppProjectDir.Value);
        EditorUtility.ClearProgressBar();
        
        Log("Done: ssl-warning-info");
    }

    private static void CopyControllerApp(string buildDir)
    {
        FileUtil.ReplaceDirectory(
            Path.Combine(ControllerAppProjectDir.Value, "build").WinToNixPath(),
            Path.Combine(buildDir, ServerSettings.Instance.ControllerAppDocumentRoot).WinToNixPath()
        );
        
        CopyWebLibDisclaimers(ControllerAppProjectDir.Value, buildDir);
    }

    private static void InitializeLibsDisclaimer(string buildDir)
    {
        var customHeader =
            "THE FOLLOWING SETS FORTH ATTRIBUTION NOTICES FOR THIRD PARTY SOFTWARE THAT MAY BE CONTAINED IN THIS PRODUCT.\n\n";
        var cSharpLibDisclaimers = ResourcePathSettings.Instance.CSharpLibDisclaimers.text;
        
        var disclaimerTargetFile = Path.Combine(buildDir, ResourcePathSettings.Instance.LibsDisclaimerFile);

        File.WriteAllText(
            disclaimerTargetFile,
            $"{customHeader}\n\n----\n\n{cSharpLibDisclaimers}\n\n-----\n\n"
        );
        
        // the remainder of this file will be filled by the copy methods of the web apps
    }

    /**
     * Copies library lists, disclaimers and attribution notices generated for a web component and
     * appends them to central files distributed with the game.
     */
    private static void CopyWebLibDisclaimers(string projectDir, string buildDir)
    {
        var listTargetFile = Path.Combine(buildDir, ResourcePathSettings.Instance.WebLibsListFile.CorrectFsSlashes());
        var listSourceFile = Path.Combine(
            projectDir,
            WebLibListBuildPath.CorrectFsSlashes()
        );
        var sourceList = File.ReadAllText(listSourceFile);
        
        File.AppendAllText(
            listTargetFile,
            sourceList
        );
        
        var disclaimerTargetFile = Path.Combine(buildDir, ResourcePathSettings.Instance.LibsDisclaimerFile.CorrectFsSlashes());
        var disclaimerSourceFile = Path.Combine(
            projectDir,
            WebLibDisclaimerBuildPath.CorrectFsSlashes()
        );
        var sourceDisclaimer = File.ReadAllText(disclaimerSourceFile);
        
        File.AppendAllText(
            disclaimerTargetFile,
            sourceDisclaimer
        );
    }
    
    private static void CopySslWarningInfoApp(string buildDir)
    {
        FileUtil.ReplaceDirectory(
            Path.Combine(SslWarningInfoAppProjectDir.Value, "build").WinToNixPath(),
            Path.Combine(buildDir, ServerSettings.Instance.SslWarningInfoDocumentRoot).WinToNixPath()
        );
        
        CopyWebLibDisclaimers(SslWarningInfoAppProjectDir.Value, buildDir);
    }

    private static void CopyCertificateFiles(string buildDir)
    {
        var certificateSourceFile =
            ServerSettings.Instance.PathToCertificatePfxForEditorMode.Replace('/', Path.DirectorySeparatorChar);
        var certificateTargetFile = Path.Combine(buildDir,
            ServerSettings.Instance.PathToCertificatePfxForBuildMode.Replace('/', Path.DirectorySeparatorChar));

        var targetDir = Directory.GetParent(certificateTargetFile).FullName;
        Directory.CreateDirectory(targetDir);
        
        FileUtil.ReplaceFile(certificateSourceFile.WinToNixPath(), certificateTargetFile.WinToNixPath());
    }

    private static void CopyReadme(string buildDir)
    {
        FileUtil.ReplaceFile(ReadmeFile.Value, Path.Combine(buildDir, "ReadMe.txt").WinToNixPath());
        
        FileUtil.ReplaceFile(DebugSettings.Instance.PathToLicense, Path.Combine(buildDir, "LICENSE.md").WinToNixPath());
    }

    private static void RunValidators()
    {
        ArtAttributionChecker.CheckCorrectArtAttribution(true);
    }
    
    private static void Log(string msg)
    {
        Debug.Log($"[ProjectBuilder] {msg}");
    }
}

#endif
