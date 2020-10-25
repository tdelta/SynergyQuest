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
using UnityEditor;
using UnityEngine;

/**
 * <summary>
 * Allows to run commands with the <c>yarn</c> cli utility.
 * </summary>
 */
public class YarnUtils
{
    public static void EnsureYarnIsInstalled()
    {
        var _ = LazyYarnExecutable.Value;
    }
    
    /**
     * <summary>
     * Runs <c>yarn install</c> in a given directory.
     * </summary>
     */
    public static void Install(string projectDirectory)
    {
        BuildUtils.RunCommand(projectDirectory, LazyYarnExecutable.Value, "install");
    }
    
    /**
     * <summary>
     * Runs <c>yarn build</c> in a given directory.
     * </summary>
     */
    public static void Build(string projectDirectory)
    {
        BuildUtils.RunCommand(projectDirectory, LazyYarnExecutable.Value, "build");
    }

    /**
     * <summary>
     * Runs <c>yarn prepare-distribution</c> in a given directory.
     * </summary>
     */
    public static void PrepareDistribution(string projectDirectory)
    {
        BuildUtils.RunCommand(projectDirectory, LazyYarnExecutable.Value, "prepare-distribution");
    }
    
    /**
     * Tries to locate yarn binary.
     */
    private static readonly Lazy<string> LazyYarnExecutable = new Lazy<string>(() =>
        {
            #if UNITY_EDITOR_WIN
                var yarnBin = "yarn.cmd";
            #else
                var yarnBin = "yarn";
            #endif
            
            Log("=== Searching PATH for Yarn executable... ===");
            var yarnPath = BuildUtils.SearchPathForFile(yarnBin);

            if (yarnPath == null)
            {
                Log("Could not find Yarn in PATH. Searching for NPM installation of yarn...");

                var globalInstallationDir = NpmUtils.GlobalInstallationDir;
                Log($"NPM installs globally to {globalInstallationDir}.");
                var possibleYarnLocation = Path.Combine(
                    Path.Combine(
                        Path.Combine(globalInstallationDir, "yarn"),
                        "bin"
                    ),
                    yarnBin
                );

                Log($"Searching Yarn at {possibleYarnLocation}...");
                if (File.Exists(possibleYarnLocation))
                {
                    Log("Found Yarn!");
                    yarnPath = possibleYarnLocation;
                }

                else
                {
                    Log("Found no NPM installation of Yarn.");
                }
            }
            
            if (yarnPath == null)
            {
                var errorMsg =
                    "Can not find yarn. Please install yarn so that it is available on the system PATH. See also https://classic.yarnpkg.com/en/docs/install/";

                Log(errorMsg);
                EditorUtility.DisplayDialog(
                    "yarn not found",
                    errorMsg,
                    "Ok"
                );
                
                throw new ApplicationException(
                    errorMsg
                );
            }

            else
            {
                Log($"Found Yarn executable at {yarnPath}.");
            }

            return yarnPath;
        }
    );
    
    private static void Log(string msg)
    {
        Debug.Log($"[{nameof(YarnUtils)}] {msg}");
    }
}
#endif
