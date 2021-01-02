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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Validation
{
    /**
     * <summary>
     * Validates that all art assets have been properly credited in the game.
     * Provides an editor menu option to run the checker.
     * </summary>
     */
    public class ArtAttributionChecker
    {
        [MenuItem("SynergyQuest Tools/Validators/Is external art properly credited?")]
        public static void CheckCorrectArtAttributionMenuItem()
        {
            CheckCorrectArtAttribution();
        }
        
        /**
         * <summary>
         * Searches the Assets/Art folders and compares the found files with the attribution notices in
         * <see cref="ResourcePathSettings.ExternalArtCredits"/> and the list of original art for the game listed in
         * <see cref="ResourcePathSettings.OriginalArtList"/>.
         * </summary>
         * <remarks>
         * If art assets are found which have not been properly credited,
         * a warning message will be displayed in the Editor and the list of found assets is printed to the debug
         * console.
         *
         * Also an exception will be thrown.
         * </remarks>
         * <param name="noConfirmationIfNoFindings">If true, do not display a confirmation dialog when no issues have been found</param>
         * <exception cref="ApplicationException">Thrown if art assets without proper credit are found.</exception>
         */
        public static void CheckCorrectArtAttribution(bool noConfirmationIfNoFindings = false)
        { 
            // get a set of all external art files which have been given proper credit
            var properlyCreditedExternalArt = GetAttributedExternalArt();
            
            // get a set of all art files which have been marked as original made for the game
            var originalArt = GetOriginalArt();

            // get all art asset files...
            var unregisteredArt = FindAllArtAssets()
                // ...and remove those that have been attributed
                .Where(assetPath => !properlyCreditedExternalArt.Contains(assetPath) && !originalArt.Contains(assetPath))
                .ToList();

            if (unregisteredArt.Any())
            {
                var warning = "There are art assets which have not been given proper credit.\n\n" +
                              $"If it is original art of the game, add it in the file {ResourcePathSettings.Instance.OriginalArtList.name}.\n" +
                              $"If it is art from an external source, add full credit information in {ResourcePathSettings.Instance.ExternalArtCredits.name}\n\n";

                Debug.LogError($"{warning}This is the full list of assets which has not been given proper credit:");
                foreach (var assetPath in unregisteredArt)
                {
                    Debug.LogError($"Missing Credit: {assetPath}");
                }

                EditorUtility.DisplayDialog(
                    "WARNING: Uncredited Art",
                    warning+
                    "A full list of non-credited assets has been listed in the Editor log.\n" +
                    "Please note that the game will NOT BUILD until all assets are appropriately credited.",
                    "Ok, I will give appropriate credit"
                );
                
                throw new ApplicationException(warning);
            }

            else if (!noConfirmationIfNoFindings)
            {
                EditorUtility.DisplayDialog(
                    "Everything OK",
                    "All external art assets have been given credit.",
                    "Ok"
                );
            }
        }
        
        private static readonly string AssetsPathPrefix = "Assets";
        
        /**
         * We will search in these paths
         */
        private static readonly string[] SearchPaths = new string[]
        {
            AssetsPathPrefix + "/Art/Fonts",
            AssetsPathPrefix + "/Art/Music",
            AssetsPathPrefix + "/Art/Sounds",
            AssetsPathPrefix + "/Art/Sprites"
        };
        
        /**
         * Files with these extensions will not be recognized as art assets
         */
        private static readonly HashSet<string> ExcludedExtensions = new HashSet<string>(new string[]
        {
            ".meta", ".md", ".asset", ".mat", ".spriteatlas"
        });

        /**
         * <summary>
         * Get a list of all art asset files marked as being originally made for the game from
         * <see cref="OriginalArtList"/>.
         * </summary>
         */
        private static HashSet<string> GetOriginalArt() {
            return new HashSet<string>(
                OriginalArtList
                    .GetOriginalArtList()
                    .Select(CleanAssetsPrefix)
            );
        }
        
        /**
         * <summary>
         * Retrieves a list of all art asset files which have been properly credited in
         * <see cref="ResourcePathSettings.ExternalArtCredits"/>.
         * </summary>
         * <remarks>
         * This method resolves wildcards in the paths listed in the file.
         * Only file level wildcards and "**" path wildcards are supported.
         * </remarks>
         */
        private static HashSet<string> GetAttributedExternalArt()
        {
            // Read asset credits information from YAML resource
            var externalArt = ExternalArtCredits
                .GetExternalArtCredits()
                .Select(entry => entry.Key)
                .SelectMany(pathPattern => // resolve wildcards
                {
                    var wildcardIndex = pathPattern.IndexOf('*');

                    if (wildcardIndex >= 0)
                    {
                        var searchPath = $"{AssetsPathPrefix}{Path.DirectorySeparatorChar}{Path.GetDirectoryName(pathPattern.Substring(0, wildcardIndex).CorrectFsSlashes())}";
                        var searchPattern = Path.GetFileName(pathPattern.CorrectFsSlashes());

                        return Directory.EnumerateFiles(
                            searchPath,
                            searchPattern,
                            SearchOption.AllDirectories
                        ).Select(path => path.WinToNixPath());
                    }

                    else
                    {
                        return new [] {pathPattern};
                    }
                })
                .Select(CleanAssetsPrefix);
            
            return new HashSet<string>(externalArt);
        }
        
        /**
         * <summary>
         * Searches all paths in <see cref="SearchPaths"/> recursively for files.
         * Excludes files with extensions listed in <see cref="ExcludedExtensions"/>.
         * </summary>
         */
        private static IEnumerable<string> FindAllArtAssets()
        {
            return SearchPaths
                .SelectMany(searchPath =>
                    Directory.EnumerateFiles(searchPath.CorrectFsSlashes(), "*", SearchOption.AllDirectories)
                )
                .Where(path => !ExcludedExtensions.Contains(Path.GetExtension(path)))
                .Select(path => path.WinToNixPath())
                .Select(CleanAssetsPrefix);
        }

        /**
         * <summary>
         * Removes leading "Assets/" prefix from paths if present
         * </summary>
         */
        private static string CleanAssetsPrefix(string path)
        {
            var prefix = AssetsPathPrefix + "/";
            if (path.StartsWith(prefix))
            {
                return path.Substring(prefix.Length);
            }

            else
            {
                return path;
            }
        }
    }
}

#endif