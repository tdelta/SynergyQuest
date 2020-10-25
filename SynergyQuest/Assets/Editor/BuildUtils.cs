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
using JetBrains.Annotations;
using Debug = UnityEngine.Debug;

/**
 * <summary>
 * Contains utility methods used in the build process of this game.
 * </summary>
 */
public class BuildUtils
{
    /**
     * <summary>
     * Tries to locate a file by searching all directories in the PATH variable.
     * </summary>
     * <returns>Full path to file or <c>null</c> if file could not be found.</returns>
     */
    [CanBeNull]
    public static string SearchPathForFile(string filename)
    {
        var envDir = Environment.GetEnvironmentVariable("PATH");
        foreach (var path in envDir.Split(Path.PathSeparator))
        {
            Log($"PATH element: {path}");
            var fullPath = Path.Combine(path, filename);

            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }
    
    /**
     * <summary>
     * Executes a command and returns stdout.
     * </summary>
     */
    public static string RunCommand(string workingDir, string executable, string args)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WorkingDirectory = workingDir,
            FileName = executable,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        process.StartInfo = startInfo;
        process.Start();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        
        Log($"Stdout of '{executable} {args}' in {workingDir}:");
        Log(stdout);

        if (process.ExitCode != 0)
        {
            throw new ApplicationException($"Non-zero exit code while running \"{executable} {args}\". Stderr: {stderr}");
        }

        return stdout;
    }
    
    private static void Log(string msg)
    {
        Debug.Log($"[{nameof(BuildUtils)}] {msg}");
    }
}

#endif
