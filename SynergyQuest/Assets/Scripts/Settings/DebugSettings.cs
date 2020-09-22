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

using System;
using UnityEngine;

/**
 * <summary>
 * Scriptable object singleton (see <see cref="ScriptableObjectSingleton{T}"/>) which allows one to
 * quickly set debug settings in the Unity editor by editing the <c>Resources/DebugSettings</c> file.
 * </summary>
 */
[CreateAssetMenu(fileName = "DebugSettings", menuName = "ScriptableObjects/DebugSettings")]
public class DebugSettings: ScriptableObjectSingleton<DebugSettings>
{
    /**
     * <summary>
     * Indicates whether the game shall be run in debug mode.
     * For example, when in debug mode, spawners spawn newly connected controllers which did not connect via lobby.
     * This variable can be manually adjusted whenever needed.
     * It is automatically set to true, if running the game in Editor mode
     *
     * For example, the <see cref="PlayerSpawner"/> class makes use of this property.
     * </summary>
     */
    public bool DebugMode => _debugMode;
    #if UNITY_EDITOR
        private bool _debugMode = true;
    #else
        private bool _debugMode = false;
    #endif

    /**
     * <summary>
     * For debugging purposes, one may want to spawn some number of locally controlled player instances in any scene
     * with a spawner.
     * 
     * This field allows to configure the number of these instances and their properties spawners shall spawn with local
     * controls.
     * This variable may be adjusted for debugging purposes.
     * </summary>
     * <seealso cref="PlayerSpawner"/>
     */
    public LocalDebugPlayerConfig[] LocalDebugPlayers => _localDebugPlayers;
    [SerializeField] private LocalDebugPlayerConfig[] _localDebugPlayers = { };

    /**
     * <summary>
     * Variable which records, whether the player instances mentioned above have been instantiated by a spawner.
     * It is set by the spawner which instantiated them.
     * </summary>
     */
    [NonSerialized] // <- This attribute is needed, so that changes to this variable are not saved to the resource
    public bool DebugPlayersWereSpawned = false;

    /**
     * <summary>
     * Every room of a dungeon should have a <see cref="DungeonLoader"/> instance which loads the dungeon files if they
     * are not loaded yet. For this purposes, the dungeon loader must be informed, for what number of players the room
     * is designed for.
     * If this number is not set, it will use the value of this field here instead.
     * </summary>
     */
    public int DefaultDungeonPlayerNum => _defaultDungeonPlayerNum;
    [SerializeField] private int _defaultDungeonPlayerNum = 2;

    /**
     * <summary>
     * Iff true, the ghost mini game on player death <see cref="ReviveMinigame"/> will not be triggered when a player
     * dies. Instead the player will respawn immediately.
     * </summary>
     */
    public bool DisableRevivalMinigame => disableRevivalMinigame;
    [SerializeField] private bool disableRevivalMinigame = false;
    
    [Tooltip("Path to file containing the game license when the game is executed in Debug mode (Unity Editor)")]
    [SerializeField] private string pathToLicenseDebugMode = "../LICENSE.md";
    
    [Tooltip("Path to file containing the game license when the game is executed in production mode (standalone executable)")]
    [SerializeField] private string pathToLicenseProductionMode = "LICENSE.md";
    
    public string PathToLicense => DebugSettings.Instance.DebugMode
      ? pathToLicenseDebugMode
      : pathToLicenseProductionMode;
}

/**
 * <summary>
 * For debugging purposes, the debug settings above allow to spawn a fixed set of player instances in the
 * game in every scene with a spawner.
 *
 * This class stores the properties each of these player instances shall be spawned with.
 * </summary>
 */
[Serializable]
public class LocalDebugPlayerConfig
{
    /**
     * Color of the player instance.
     */
    public PlayerColor color;
    
    /**
     * Which local input controls shall be used for the player?
     */
    public LocalInput inputPrefab;
}
