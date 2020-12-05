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

using UnityEngine;

/**
 * <summary>
 * Stores names and paths of miscellaneous resource files. 
 * </summary>
 * <remarks>
 * This is a scriptable object singleton (see <see cref="ScriptableObjectSingleton{T}"/>) which allows one to
 * quickly set settings in the Unity editor by editing the <c>Resources/ResourcePathSettings</c> file.
 * </remarks>
 */
[CreateAssetMenu(fileName = "ResourcePathSettings", menuName = "ScriptableObjects/ResourcePathSettings")]
public class ResourcePathSettings: ScriptableObjectSingleton<ResourcePathSettings>
{
    /**
     * <summary>
     * Name of the dungeon layout file which shall be loaded by the game when exiting the lobby and
     * starting the game.
     * </summary>
     */
    public string InitialDungeonLayoutFile => initialDungeonLayoutFile;
    [SerializeField] private string initialDungeonLayoutFile = "MainDungeon.json";
    
    /**
     * A text resource YAML file containing a list of all external art used in the game with license info etc.
     */
    public TextAsset ExternalArtCredits => externalArtCredits;
    [SerializeField] private TextAsset externalArtCredits;
    
    /**
     * A text resource YAML file containing a list of all art originally made for the game.
     * That is, no explicit attribution information must be displayed for this art.
     */
    public TextAsset OriginalArtList => originalArtList;
    [SerializeField] private TextAsset originalArtList; 
    
    /**
     * A text resource file containing the licenses and disclaimers of all used C# libs
     * (except the Unity stuff)
     */
    public TextAsset CSharpLibDisclaimers => cSharpLibDisclaimers;
    [SerializeField] private TextAsset cSharpLibDisclaimers;
    
    /**
     * <summary>
     * Name of a file containing a list of all used web libraries.
     * (It is placed automatically in the installation directory of the game when creating a production build.)
     * </summary>
     */
    public string WebLibsListFile => webLibsListFile;
    [SerializeField]
    [Tooltip("Relative path to the file containing a list of all used web libraries. Relative to the installation dir of the game.")]
    private string webLibsListFile = "web-libs-list.txt";

    /**
     * <summary>
     * Name of the disclaimer file for external libraries.
     * (This file is created when creating a production build of the game).
     * </summary>
     */
    public string LibsDisclaimerFile => libsDisclaimerFile;
    [SerializeField]
    [Tooltip("Relative path to the disclaimer file for all used external libraries. Relative to the installation dir of the game.")]
    private string libsDisclaimerFile = "libs-disclaimers.txt";
}