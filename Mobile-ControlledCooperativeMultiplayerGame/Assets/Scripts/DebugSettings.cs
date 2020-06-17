public static class DebugSettings
{
    /**
     * Indicates whether the game shall be run in debug mode.
     * For example, when in debug mode, spawners spawn newly connected controllers which did not connect via lobby.
     * This variable must be manually adjusted whenever needed.
     *
     * See also `PlayerSpawner`
     */
    private static bool _debugMode = false;
    public static bool DebugMode => _debugMode;

    /**
     * Number of players and their color every spawner shall spawn with local controls.
     * This variable may be adjusted for debugging purposes.
     *
     * See also `PlayerSpawner`
     */
    private static PlayerColor[] _localDebugPlayers = { };// {PlayerColor.Blue};
    public static PlayerColor[] LocalDebugPlayers => _localDebugPlayers;
}
