using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

/**
 * A dungeon can be defined as a graph of connected rooms using a `.json` file.
 * This singleton class reads such layout files and contains the logic to load connected rooms when using doors.
 * The same room may be implemented multiple times for a different number of players.
 *
 * # The Layout File
 *
 * A dungeon layout is defined by a set of rooms and the name of the initial room.
 * Every room definition consists of a set of doors.
 * Every door has an id and its definition consists of the name of the target room and the id of the door in that room
 * it leads to.
 *
 * Hence, a layout file looks like this:
 * ```json
 * {
 *     "initialRoom": "NameOfInitialRoom",
 *     "rooms": {
 *         "RoomName1": {
 *             "roomConnections": {
 *                 "DoorId1": {
 *                     "targetRoom": "TargetRoomName",
 *                     "targetDoor": "TargetDoorId"
 *                 },
 *                 "DoorId2": ...
 *             }
 *         },
 *         "RoomName2": ...
 *     }
 * }
 * ```
 *
 * # Dungeon Folder Structure
 *
 * This singleton will search for the scene files of rooms in the subdirectories of the directory where the layout .json
 * file was placed.
 * For every room "DX" for every number "N" of players supported must be implemented as a Unity scene which has been
 * added to the build settings and is saved in the subdirectory "rooms/DX/DX_PN.unity". 
 */
public class DungeonLayout : Singleton<DungeonLayout>
{
    /**
     * Data loaded from the JSON layout file
     */
    private DungeonLayoutData _data;
    /**
     * Path to the folder containing the dungeon files
     */
    private string _dungeonPath;
    /**
     * Number of players for which the dungeon is loaded
     */
    private int _playerNum;
    /**
     * Currently loaded room
     */
    private string _currentRoom;
    /**
     * Which door of the room has been used by players to enter it.
     * Players will be spawned at its spawner object, see also the `DoorSpawner" behavior.
     */
    private int _spawnerDoorId = InvalidDoorId;

    /**
     * Whether a dungeon layout has been loaded
     */
    public bool IsLoaded => _currentRoom != null;
    public int SpawnerDoorId => _spawnerDoorId;
    public const int InvalidDoorId = -1;
    
    /**
     * Whether a door has been used to enter the current room.
     * If not, the default spawner of the room shall be used to spawn players instead
     * (see also the `RoomDefaultPlayerSpawner` class).
     */
    public bool IsUsingDoorAsSpawner => SpawnerDoorId != InvalidDoorId;

    /**
     * Opens a dungeon layout file and loads the initial room.
     *
     * @param filePath             path to the .json layout file
     * @param playerNum            number of players for which the dungeon shall be loaded
     * @param overwriteInitialRoom if not null, this room will be loaded initially, instead of the one specified
     *                             in the layout file
     * @param doNotLoadScene       if true, the dungeon layout file is loaded, but the current scene is not changed to
     *                             the initial room
     */
    public void LoadDungeon(
        string filePath,
        int playerNum,
        string overwriteInitialRoom = null,
        bool doNotLoadScene = false
    ) {
        _playerNum = playerNum;
        _data = DungeonLayoutData.FromFile(filePath);
        _dungeonPath = Path.GetDirectoryName(filePath);

        var roomToLoad = overwriteInitialRoom ?? _data.initialRoom;
        if (doNotLoadScene)
        {
            _currentRoom = roomToLoad;
        }

        else
        {
            LoadRoomByName(roomToLoad);
        }
    }

    /**
     * Loads the room the given door is connected to.
     * The dungeon layout must already be loaded and the door must belong to the layout, see also the `LoadDungeon`
     * method and the `Door` class.
     *
     * @throws InvalidOperationException if no layout data is currently loaded or the loaded layout data is
     *                                   inconsistent / erroneous
     * @throws ArgumentException         if the current room does not contain data of a door matching the given one
     */
    public void LoadRoomUsingDoor(Door door)
    {
        EnsureLayoutIsLoaded();
        
        // Retrieve the data of the currently loaded room
        if (_data.rooms.TryGetValue(_currentRoom, out var roomData)) 
        {
            // Retrieve data stored for the door in the layout file
            if (roomData.roomConnections.TryGetValue(door.DoorId, out var doorData))
            {
                // Load the room the door points to
                LoadRoomByName(doorData.targetRoom, door.TransitionType, doorData.targetDoor);
            }

            else
            {
                throw new ArgumentException("Can not use door, since the current room has no door with that ID. Maybe the current scene does not belong to the dungeon layout or contains errors?");
            }
        }

        else
        {
            throw new InvalidOperationException("Can not load room using the given door, since the layout file seems to be inconsistent as it does not contain the current room.");
        }
    }

    /**
     * Loads a specific room in the layout by its name.
     *
     * @param roomName       name of the room to load
     * @param transitionType the type of transition animation to play when loading the room (optional)
     * @param targetDoor     the door in the loaded room, where players shall spawn.
     *                       If set to `InvalidDoorId`, a default spawner will be used instead
     *                       (see also `RoomDefaultPlayerSpawner`). Optional parameter.
     */
    private void LoadRoomByName(string roomName, TransitionType transitionType = TransitionType.None, int targetDoor = InvalidDoorId)
    {
        SceneController.Instance.LoadSceneByName(
            SceneNameFromRoomName(roomName),
            transitionType,
            () =>
            {
                // As soon as the new room scene has been loaded, refresh the state of this singleton accordingly:
                _currentRoom = roomName;
                _spawnerDoorId = targetDoor;
            }
        );
    }

    /**
     * Computes the expected file path of a room scene, given the name of a room.
     * For details on where the room files are expected to be located, see the class description.
     */
    private string SceneNameFromRoomName(string roomName)
    {
        var sep = Path.DirectorySeparatorChar;
        
        // This variable will store the path to the folder where all scene files of the dungeon layout are located
        string dungeonScenePath = _dungeonPath;

        // If the path starts with the "Assets" folder, we remove it from the path, since Unity will automatically
        // assume all scene files to be located in the Assets folder and expects paths to be relative to it.
        var assetsRelPath = $"Assets{sep}";
        var assetsIdx = _dungeonPath.IndexOf(assetsRelPath);
        if (assetsIdx != -1)
        {
            dungeonScenePath = dungeonScenePath.Remove(assetsIdx, assetsRelPath.Length);
        }
        
        // Return the full path where the room scene file for the current number of players is expected
        return Path.Combine(dungeonScenePath, $"rooms{sep}{roomName}{sep}{roomName}_P{_playerNum}");
    }

    /**
     * Throws an InvalidOperationException if no dungeon layout is currently loaded.
     */
    private void EnsureLayoutIsLoaded()
    {
        if (_data == null || _currentRoom == null)
        {
            throw new InvalidOperationException("No dungeon layout file is loaded.");
        }
    }
}

/**
 * Represents the JSON data format for a door definition.
 * See the class description of `DungeonLayout` for more information.
 */
class DoorData
{
    public string targetRoom;
    public int targetDoor;
}

/**
 * Represents the JSON data format for a room definition.
 * See the class description of `DungeonLayout` for more information.
 */
class DungeonRoomData
{
    public Dictionary<int, DoorData> roomConnections;
}

/**
 * Represents the JSON data format for a dungeon layout definition.
 * See the class description of `DungeonLayout` for more information.
 */
class DungeonLayoutData
{
    public string initialRoom;
    public Dictionary<string, DungeonRoomData> rooms;

    /**
     * Loads a dungeon layout definition from a file.
     */
    public static DungeonLayoutData FromFile(string filePath)
    {
        // Use the JSON.Net library for loading, since the Unity built in library does not support
        // dictionaries.
        var layout = JsonConvert.DeserializeObject<DungeonLayoutData>(File.ReadAllText(filePath));

        return layout;
    }
}
