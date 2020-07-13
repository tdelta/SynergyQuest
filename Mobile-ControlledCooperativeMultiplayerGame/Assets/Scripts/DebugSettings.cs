using System;
using UnityEngine;

[Serializable]
public class LocalDebugPlayerConfig
{
    public PlayerColor color;
    /**
     * If players with local controls are spawned for debugging, which local input controls shall be used?
     * When spawning multiple locally controlled players, we will cycle through this list.
     */
    public LocalInput inputPrefab;
}

[CreateAssetMenu(fileName = "DebugSettings", menuName = "ScriptableObjects/DebugSettings")]
public class DebugSettings: ScriptableObjectSingleton<DebugSettings>
{
    /**
     * Indicates whether the game shall be run in debug mode.
     * For example, when in debug mode, spawners spawn newly connected controllers which did not connect via lobby.
     * This variable can be manually adjusted whenever needed.
     * By default, it is set to true, if running the game in Editor mode
     *
     * See also `PlayerSpawner`
     */
    #if UNITY_EDITOR
        private bool _debugMode = true;
    #else
        private bool _debugMode = false;
    #endif
    public bool DebugMode => _debugMode;

    /**
     * Number of players and their color every spawner shall spawn with local controls.
     * This variable may be adjusted for debugging purposes.
     *
     * See also `PlayerSpawner`
     */
    [SerializeField] private LocalDebugPlayerConfig[] _localDebugPlayers = { };
    public LocalDebugPlayerConfig[] LocalDebugPlayers => _localDebugPlayers;

    [NonSerialized] // <- This attribute is needed, so that changes to this variable are not saved to the resource
    public bool DebugPlayersWereSpawned = false;

    [SerializeField] private int _defaultDungeonPlayerNum = 2;
    public int DefaultDungeonPlayerNum => _defaultDungeonPlayerNum;
}
