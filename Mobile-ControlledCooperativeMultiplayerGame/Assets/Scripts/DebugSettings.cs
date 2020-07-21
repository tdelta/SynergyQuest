using System;
using UnityEngine;

/**
 * Scriptable object singleton (see `ScriptableObjectSingleton`) which allows one to
 * quickly set debug settings in the Unity editor by editing the `Resources/DebugSettings` file.
 */
[CreateAssetMenu(fileName = "DebugSettings", menuName = "ScriptableObjects/DebugSettings")]
public class DebugSettings: ScriptableObjectSingleton<DebugSettings>
{
    /**
     * Indicates whether the game shall be run in debug mode.
     * For example, when in debug mode, spawners spawn newly connected controllers which did not connect via lobby.
     * This variable can be manually adjusted whenever needed.
     * It is automatically set to true, if running the game in Editor mode
     *
     * For example, the `PlayerSpawner` class makes use of this property.
     */
    #if UNITY_EDITOR
        private bool _debugMode = true;
    #else
        private bool _debugMode = false;
    #endif
    public bool DebugMode => _debugMode;

    /**
     * For debugging purposes, one may want to spawn some number of locally controlled player instances in any scene
     * with a spawner.
     * 
     * This field allows to configure the number of these instances and their properties spawners shall spawn with local
     * controls.
     * This variable may be adjusted for debugging purposes.
     *
     * See also `PlayerSpawner`
     */
    [SerializeField] private LocalDebugPlayerConfig[] _localDebugPlayers = { };
    public LocalDebugPlayerConfig[] LocalDebugPlayers => _localDebugPlayers;

    /**
     * Variable which records, whether the player instances mentioned above have been instantiated by a spawner.
     * It is set by the spawner which instantiated them.
     */
    [NonSerialized] // <- This attribute is needed, so that changes to this variable are not saved to the resource
    public bool DebugPlayersWereSpawned = false;

    /**
     * Every room of a dungeon should have a `DungeonLoader` instance which loads the dungeon files if they are not
     * loaded yet. For this purposes, the dungeon loader must be informed, for what number of players the room is
     * designed for.
     * If this number is not set, it will use the value of this field here instead.
     */
    [SerializeField] private int _defaultDungeonPlayerNum = 2;
    public int DefaultDungeonPlayerNum => _defaultDungeonPlayerNum;
}

/**
 * For debugging purposes, the debug settings above allow to spawn a fixed set of player instances in the
 * game in every scene with a spawner.
 *
 * This class stores the properties each of these player instances shall be spawned with.
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
