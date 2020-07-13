using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

class DoorData
{
    public string targetRoom;
    public int targetDoor;
}

class DungeonRoomData
{
    public Dictionary<int, DoorData> roomConnections;
}

class DungeonLayoutData
{
    public string initialRoom;
    public Dictionary<string, DungeonRoomData> rooms;

    public static DungeonLayoutData FromFile(string filePath)
    {
        var layout = JsonConvert.DeserializeObject<DungeonLayoutData>(File.ReadAllText(filePath));

        return layout;
    }
}

public class DungeonLayout : Singleton<DungeonLayout>
{
    private DungeonLayoutData _data;
    private string _dungeonPath;
    private int _playerNum;
    private string _currentRoom;
    private int _spawnerDoorId = InvalidDoorId;

    public bool IsLoaded => _currentRoom != null;
    public int SpawnerDoorId => _spawnerDoorId;
    public const int InvalidDoorId = -1;
    public bool IsUsingDoorAsSpawner => SpawnerDoorId != InvalidDoorId;

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

    public void LoadRoomUsingDoor(Door door)
    {
        if (_data.rooms.TryGetValue(_currentRoom, out var roomData)) 
        {
            if (roomData.roomConnections.TryGetValue(door.DoorId, out var doorData))
            {
                LoadRoomByName(doorData.targetRoom, door.TransitionType, doorData.targetDoor);
            }

            else
            {
                throw new ArgumentException("Can not use door, since the current room has no door with that ID.");
            }
        }

        else
        {
            throw new ArgumentException("Can not use door, if no dungeon room is currently loaded.");
        }
    }

    public void LoadRoomByName(string roomName, TransitionType transitionType = TransitionType.None, int targetDoor = InvalidDoorId)
    {
        SceneController.Instance.LoadSceneByName(
            SceneNameFromRoomName(roomName),
            transitionType,
            () =>
            {
                _currentRoom = roomName;
                _spawnerDoorId = targetDoor;
            }
        );
    }

    private string SceneNameFromRoomName(string roomName)
    {
        var sep = Path.DirectorySeparatorChar;
        
        string dungeonScenePath = _dungeonPath;

        var assetsRelPath = $"Assets{sep}";
        var assetsIdx = _dungeonPath.IndexOf(assetsRelPath);
        if (assetsIdx != -1)
        {
            dungeonScenePath = dungeonScenePath.Remove(assetsIdx, assetsRelPath.Length);
        }
        
        return Path.Combine(dungeonScenePath, $"rooms{sep}{roomName}{sep}{roomName}_P{_playerNum}");
    }
}
