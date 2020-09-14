using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

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
 * For every room "DX" for every number "N" of supported players a scene must be implemented. It must be
 * added to the build settings and be saved in the subdirectory "rooms/DX/DX_PN.unity".
 * If you want to develop just one scene for every number of players, leave out the "_PN" suffix and the scene will
 * be used as fallback, whenever no scene with the right numeric suffix is present.
 */
public class DungeonLayout : Singleton<DungeonLayout>
{
    /**
     * Data loaded from the JSON layout file
     */
    private DungeonLayoutData _data;
    public DungeonLayoutData Data => _data;

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
    public string CurrentRoom => _currentRoom;

    /**
     * Which door of the room has been used by players to enter it.
     * Players will be spawned at its spawner object, see also the `DoorSpawner" behavior.
     */
    private string _spawnerDoorId = InvalidDoorId;

    /**
     * Whether a dungeon layout has been loaded
     */
    public bool IsLoaded => _currentRoom != null;
    public string SpawnerDoorId => _spawnerDoorId;
    public const string InvalidDoorId = null;
    
    /**
     * Whether a door has been used to enter the current room.
     * If not, the default spawner of the room shall be used to spawn players instead
     * (see also the `RoomDefaultPlayerSpawner` class).
     */
    public bool IsUsingDoorAsSpawner => SpawnerDoorId != InvalidDoorId;

    /**
     * <summary>
     * Opens a dungeon layout file and loads the initial room.
     * </summary>
     * <param name="filePath">
     *     relative path to a .json layout file located in <c>Resources/DungeonLayouts</c>
     * </param>
     * <param name="playerNum">
     *     number of players for which the dungeon shall be loaded
     * </param>
     * <param name="overwriteInitialRoom">
     *     if not null, this room will be loaded initially, instead of the one specified
     *     in the layout file
     * </param>
     * <param name="doNotLoadScene">
     *     if true, the dungeon layout file is loaded, but the current scene is not changed to
     *     the initial room
     * </param>
     */
    public void LoadDungeon(
        string filePath,
        int playerNum,
        string overwriteInitialRoom = null,
        bool doNotLoadScene = false
    ) {
        _playerNum = playerNum;
        _data = DungeonLayoutData.FromResource($"DungeonLayouts/{filePath}");
        _dungeonPath = Path.GetDirectoryName(filePath).WinToNixPath();

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
     * Convert room name to scene name and return its data in the layout
     *
     * @param roomName    name of the room to find
     */
    public DungeonRoomData GetRoomByName(string roomName)
    {
        return _data.rooms[SceneNameFromRoomName(roomName)];
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
    private void LoadRoomByName(string roomName, TransitionType transitionType = TransitionType.None, string targetDoor = InvalidDoorId)
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
        var sep = "/";
        
        var scenePrefixPath = $"{_data.scenesPath}{sep}{roomName}{sep}{roomName}";
        var scenePath = $"{scenePrefixPath}_P{_playerNum}";
        
        // If no scene is at the computed path, fall back to a path without a player number
        if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
        {
            scenePath = scenePrefixPath;
        }
        
        // Return the full path where the room scene file for the current number of players is expected
        return scenePath;
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
public class DoorData
{
    public string targetRoom = default;
    public string targetDoor = default;
}

/**
 * Represents the JSON data format for a room definition.
 * See the class description of `DungeonLayout` for more information.
 */
public class DungeonRoomData
{
    public Dictionary<string, DoorData> roomConnections = default;
}

/**
 * <summary>
 * Represents the JSON data format for a dungeon layout definition.
 * See the class description of <see cref="DungeonLayout"/> for more information.
 * </summary>
 */
public class DungeonLayoutData
{
    /**
     * <summary>
     * Path to the scene asset folder where all scenes of the dungeon are located.
     * It must be a relative path inside the "Assets" folder.
     * </summary>
     */
    public string scenesPath = default;
    public string initialRoom = default;
    public Dictionary<string, DungeonRoomData> rooms = default;

    /**
     * <summary>
     * Loads a dungeon layout definition from a layout file.
     * The layout file must be stored as a resource in the <c>Resources</c> folder and the given path
     * must be relative to that folder.
     * </summary>
     */
    public static DungeonLayoutData FromResource(string filePath)
    {
        // Use the JSON.Net library for loading, since the Unity built in library does not support
        // dictionaries.
        var layout = JsonConvert.DeserializeObject<DungeonLayoutData>(
            Resources.Load<TextAsset>(
                // Resources must be loaded without file extension
                $"{Path.GetDirectoryName(filePath).WinToNixPath()}/{Path.GetFileNameWithoutExtension(filePath)}"
            ).text
        );

        return layout;
    }
}
