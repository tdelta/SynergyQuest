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
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    private static string YarnExecutable {
        get {
            #if UNITY_EDITOR_WIN
                // try to find yarn on PATH
                string yarnExecutable = null;
                
                var envDir = Environment.GetEnvironmentVariable("PATH");
                foreach (var path in envDir.Split(Path.PathSeparator))
                {
                    var fullPath = Path.Combine(path, "yarn.cmd");

                    if (File.Exists(fullPath))
                    {
                        yarnExecutable = fullPath;
                    }
                }

                if (yarnExecutable == null)
                {
                    var errorMsg =
                        "Can not find yarn. Please install yarn so that it is available on the system PATH. See also https://classic.yarnpkg.com/en/docs/install/#windows-stable";

                    EditorUtility.DisplayDialog(
                        "yarn not found",
                        errorMsg,
                        "Ok"
                    );
                    
                    throw new ApplicationException(
                        errorMsg
                    );
                }

                return yarnExecutable;
            #else
                return "yarn";
            #endif
        }
    }
    
    // ReSharper disable once PossibleNullReferenceException
    private static readonly Lazy<string> ProjectDir = new Lazy<string>(() => Directory.GetParent(Application.dataPath).Parent.FullName);
    
    private static readonly Lazy<string> GameProjectDir = new Lazy<string>(() => Directory.GetParent(Application.dataPath).FullName);
    
    private static readonly Lazy<string> ControllerProjectsDir = new Lazy<string>(() => Path.Combine(ProjectDir.Value, "Controller"));
    private static readonly Lazy<string> ControllerClientLibProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "controller-client-lib"));
    private static readonly Lazy<string> SensorInputLibProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "sensor-input-lib"));
    private static readonly Lazy<string> ControllerAppProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "controller-app"));
    private static readonly Lazy<string> SslWarningInfoAppProjectDir = new Lazy<string>(() => Path.Combine(ControllerProjectsDir.Value, "ssl-warning-info"));
    
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
    [MenuItem("SynergyQuest Build/Clean Builds")]
    public static void CleanBuilds()
    {
        FileUtil.DeleteFileOrDirectory(BuildDirectory.Value.WinToNixPath());
        
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
    [MenuItem("SynergyQuest Build/Windows Build")]
    public static void BuildForWindows ()
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

        try
        {
            BuildControllerApp();
            CopyControllerApp(WindowsBuildDirectory.Value);
            
            BuildSslWarningInfoApp();
            CopySslWarningInfoApp(WindowsBuildDirectory.Value);
            
            CopyCertificateFiles(WindowsBuildDirectory.Value);
            
            CopyReadme(WindowsBuildDirectory.Value);
        }

        catch
        {
            EditorUtility.ClearProgressBar();
            throw;
        }
        
        EditorUtility.DisplayDialog(
            "Windows Build finished",
            $"The build can be found at {WindowsBuildDirectory.Value}.",
            "Ok"
        );
    }
    
    /**
     * <summary>
     * Builds and bundles a standalone version of SynergyQuest for Linux
     * (Including all web libraries and apps etc.)
     * </summary>
     */
    [MenuItem("SynergyQuest Build/Linux Build")]
    public static void BuildForLinux ()
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

        try
        {
            BuildControllerApp();
            CopyControllerApp(LinuxBuildDirectory.Value);
            
            BuildSslWarningInfoApp();
            CopySslWarningInfoApp(LinuxBuildDirectory.Value);
            
            CopyCertificateFiles(LinuxBuildDirectory.Value);
            
            CopyReadme(LinuxBuildDirectory.Value);
        }
            
        catch
        {
            EditorUtility.ClearProgressBar();
            throw;
        }

        var title = "=== Linux Build finished ===";
        var msg = $"The build can be found at {LinuxBuildDirectory.Value}.";
        
        Log(title);
        Log(msg);
        EditorUtility.DisplayDialog(
            title,
            msg,
            "Ok"
        );
    }

    // === Helper methods for building sub-components ===
    
    private static void BuildControllerClientLib()
    {
        var title = "=== Building controller-client-lib with yarn ===";
        Log(title);

        var msg1 = "Installing dependencies for controller-client-lib";
        EditorUtility.DisplayProgressBar(title, msg1, 0.0f);
        Log(msg1);
        RunCommand(ControllerClientLibProjectDir.Value, YarnExecutable, "install");

        var msg2 = "Building controller-client-lib";
        EditorUtility.DisplayProgressBar(title, msg1, 0.5f);
        Log(msg2);
        RunCommand(ControllerClientLibProjectDir.Value, YarnExecutable, "build");
        
        EditorUtility.ClearProgressBar();
        
        Log("Done: controller-client-lib");
    }
    
    private static void BuildSensorInputLib()
    {
        var title = "=== Building sensor-input-lib with yarn ===";
        Log(title);

        var msg1 = "Installing dependencies for sensor-input-lib";
        EditorUtility.DisplayProgressBar(title, msg1, 0.0f);
        Log(msg1);
        RunCommand(SensorInputLibProjectDir.Value, YarnExecutable, "install");


        var msg2 = "Building sensor-input-lib";
        EditorUtility.DisplayProgressBar(title, msg2, 0.5f);
        Log(msg2);
        RunCommand(SensorInputLibProjectDir.Value, YarnExecutable, "build");
        
        EditorUtility.ClearProgressBar();
        
        Log("Done: sensor-input-lib");
    }
    
    private static void BuildControllerApp()
    {
        BuildControllerClientLib();
        BuildSensorInputLib();
        
        var title = "=== Building main controller web app with yarn ===";
        Log(title);

        var msg1 = "Installing dependencies for controller-app";
        EditorUtility.DisplayProgressBar(title, msg1, 0.0f);
        Log(msg1);
        RunCommand(ControllerAppProjectDir.Value, YarnExecutable, "install");
        
        var msg2 = "Upgrading dependency on controller-client-lib";
        EditorUtility.DisplayProgressBar(title, msg2, 0.25f);
        Log(msg2);
        RunCommand(ControllerAppProjectDir.Value, YarnExecutable, $"upgrade ..{Path.DirectorySeparatorChar}controller-client-lib");
        
        var msg3 = "Upgrading dependency on sensor-input-lib";
        EditorUtility.DisplayProgressBar(title, msg3, 0.5f);
        Log(msg3);
        RunCommand(ControllerAppProjectDir.Value, YarnExecutable, $"upgrade ..{Path.DirectorySeparatorChar}sensor-input-lib");
        
        var msg4 = "Building controller-app";
        EditorUtility.DisplayProgressBar(title, msg4, 0.75f);
        Log(msg4);
        RunCommand(ControllerAppProjectDir.Value, YarnExecutable, "build");
        
        EditorUtility.ClearProgressBar();
    }
    
    private static void BuildSslWarningInfoApp()
    {
        RunCommand(SslWarningInfoAppProjectDir.Value, YarnExecutable, "install");
        RunCommand(SslWarningInfoAppProjectDir.Value, YarnExecutable, "build");
    }

    private static void CopyControllerApp(string buildDir)
    {
        FileUtil.ReplaceDirectory(
            Path.Combine(ControllerAppProjectDir.Value, "build").WinToNixPath(),
            Path.Combine(buildDir, ServerSettings.Instance.ControllerAppDocumentRoot).WinToNixPath()
        );
    }
    
    private static void CopySslWarningInfoApp(string buildDir)
    {
        FileUtil.ReplaceDirectory(
            Path.Combine(SslWarningInfoAppProjectDir.Value, "build").WinToNixPath(),
            Path.Combine(buildDir, ServerSettings.Instance.SslWarningInfoDocumentRoot).WinToNixPath()
        );
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

    private static void RunCommand(string workingDir, string executable, string args)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = workingDir,
            FileName = executable,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        process.StartInfo = startInfo;
        process.Start();

        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new ApplicationException($"Non-zero exit code while running \"{executable} {args}\". Stderr: {stderr}");
        }
    }

    private static void Log(string msg)
    {
        Debug.Log($"[ProjectBuilder] {msg}");
    }
}

#endif
