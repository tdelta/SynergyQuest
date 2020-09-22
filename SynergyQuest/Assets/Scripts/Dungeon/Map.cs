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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Contains a 2D List of "cells" that can be used to draw a dungeon map
 * </summary>
 */
public class Map
{
    public List<List<Optional<DungeonRoomViewData>>> RoomMap = new List<List<Optional<DungeonRoomViewData>>>();

    public int RowCount { get; private set; }
    public int ColumnCount { get; private set; }

    /**
     * Parse the loaded dungeon layout into a 2D grid
     * 
     * TODO: Rooms can only have one door per direction, probably better to extract direction from tilemap
     */
    public void ParseDungeon()
    {
        DungeonRoomViewData initialRoom = new DungeonRoomViewData(
            DungeonLayout.Instance.Data.initialRoom,
            new Vector2Int(0, 0)
            );
        RoomMap = new List<List<Optional<DungeonRoomViewData>>>();
        RoomMap.Add(new List<Optional<DungeonRoomViewData>>());
        RoomMap[0].Add(Optional<DungeonRoomViewData>.Some(initialRoom));
        
        ParseDungeonRec(initialRoom);

        RoomMap.Reverse();
        ReassignCoordinates();

        RowCount = RoomMap.Count;
        ColumnCount = RoomMap.Select(row => row.Count).Max();
    }

    private void ParseDungeonRec(DungeonRoomViewData roomData)
    {
        var doors = DungeonLayout.Instance.Data.rooms[roomData.name].roomConnections;
        foreach (var direction in new[] {"up", "down", "left", "right"})
        {
            if (doors.TryGetValue(direction, out var connectedRoom))
            {
                if (!Contains(connectedRoom.targetRoom))
                {
                    DungeonRoomViewData connectedRoomData = null; // Cannot be null after switch
                    switch (direction)
                    {
                        case "up":
                            connectedRoomData = InsertAbove(connectedRoom.targetRoom, roomData.coordinates);
                            break;
                        case "down":
                            connectedRoomData = InsertBelow(connectedRoom.targetRoom, roomData.coordinates);
                            break;
                        case "left":
                            connectedRoomData = InsertLeft(connectedRoom.targetRoom, roomData.coordinates);
                            break;
                        case "right":
                            connectedRoomData = InsertRight(connectedRoom.targetRoom, roomData.coordinates);
                            break;
                    }

                    roomData.connections.Add(connectedRoomData);
                    ParseDungeonRec(connectedRoomData);
                }
                else
                {
                    // Simply search the room in the map and add a connection
                    Func<Optional<DungeonRoomViewData>, bool> predicate = o => o.Fulfills(r => r.name == connectedRoom.targetRoom);
                    var connectedRoomData = RoomMap
                        .Find(row => row.Any(predicate))
                        .Find(o => o.Fulfills(r => r.name == connectedRoom.targetRoom));
                    connectedRoomData.Match(r => roomData.connections.Add(r), () => { });
                }
            }
        }
    }

    private DungeonRoomViewData Insert(string room, Vector2Int coordinates)
    {
        var data = new DungeonRoomViewData(room, coordinates);
        RoomMap[coordinates.y][coordinates.x] = Optional<DungeonRoomViewData>.Some(data);
        return data;
    }

    /**
     * Map contains a room with the given name
     */
    private bool Contains(string roomName)
    {
        bool found = false;
        foreach (var row in RoomMap)
        {
            foreach (var room in row)
            {
                room.Match(r => found = found || r.name == roomName, () => { });
                if (found) return true;
            }
        }

        return found;
    }
    
    /**
     * Insert a new room into the map. Expand the map if necessary
     *
     * <param name="room">Name of the new room to insert</param>
     * <param name="coordinates">Coordinates above which the new room should be inserted</param>
     */
    private DungeonRoomViewData InsertAbove(string room, Vector2Int coordinates)
    {
        if (RoomMap.Count <= coordinates.y + 1 || RoomMap[coordinates.y + 1][coordinates.x].IsSome())
        {
            // We either hit the edge of the current map or a room to the top
            ExpandVertically(coordinates.y + 1);
        }
        return Insert(room,new Vector2Int(coordinates.x, coordinates.y + 1));
    }
    
    /**
     * Insert a new room into the map. Expand the map if necessary
     *
     * <param name="room">Name of the new room to insert</param>
     * <param name="coordinates">Coordinates below which the new room should be inserted</param>
     */
    private DungeonRoomViewData InsertBelow(string room, Vector2Int coordinates)
    {
        Vector2Int newCoordinates;
        if (coordinates.y == 0 || RoomMap[coordinates.y - 1][coordinates.x].IsSome())
        {
            // We either hit the edge of the current map or a room to the bottom
            ExpandVertically(coordinates.y);
            newCoordinates = coordinates;
        }
        else
        {
            newCoordinates = new Vector2Int(coordinates.x, coordinates.y - 1);
        }
        return Insert(room, newCoordinates);
    }

    /**
     * Insert a new room into the map. Expand the map if necessary
     *
     * <param name="room">Name of the new room to insert</param>
     * <param name="coordinates">Coordinates left of which the new room should be inserted</param>
     */
    private DungeonRoomViewData InsertLeft(string room, Vector2Int coordinates)
    {
        Vector2Int newCoordinates;
        if (coordinates.x == 0 || RoomMap[coordinates.y][coordinates.x - 1].IsSome())
        {
            // We either hit the edge of the current map or a room to the left
            ExpandHorizontally(coordinates.x);
            newCoordinates = coordinates;
        }
        else
        {
            newCoordinates = new Vector2Int(coordinates.x - 1, coordinates.y);
        }
        return Insert(room, newCoordinates);
    }

    /**
     * Insert a new room into the map. Expand the map if necessary
     *
     * <param name="room">Name of the new room to insert</param>
     * <param name="coordinates">Coordinates right of which the new room should be inserted</param>
     */
    private DungeonRoomViewData InsertRight(string room, Vector2Int coordinates)
    {
        if (RoomMap[coordinates.y].Count <= coordinates.x + 1 || RoomMap[coordinates.y][coordinates.x + 1].IsSome())
        {
            // We either hit the edge of the current map or a room to the right 
            ExpandHorizontally(coordinates.x + 1);
        }
        return Insert(room, new Vector2Int(coordinates.x + 1, coordinates.y));
    }

    /**
     * Insert a new row into the map
     *
     * <param name="yIndex">Index of the new row</param>
     */
    private void ExpandVertically(int yIndex)
    {
        if (RoomMap.Count == 0) throw new ArgumentException("Cant expand an empty map");

        RoomMap.Insert(yIndex, new List<Optional<DungeonRoomViewData>>());
        int width;
        if (yIndex == 0) width = RoomMap[1].Count;
        else width = RoomMap[0].Count;

        for (int i = 0; i < width; i++)
        {
            RoomMap[yIndex].Add(Optional<DungeonRoomViewData>.None());
        }
        ReassignCoordinates();
        StrechConnections();
    }

    /**
     * Insert a new column into the map
     *
     * <param name="xIndex">Index of the new column</param>
     */
    private void ExpandHorizontally(int xIndex)
    {
        if (RoomMap.Count == 0) throw new ArgumentException("Cant expand an empty map");

        foreach (var row in RoomMap)
        {
            row.Insert(xIndex, Optional<DungeonRoomViewData>.None());
        }
        ReassignCoordinates();
        StrechConnections();
    }

    /**
     * Assign the correct coordinates in the map to each room
     */
    private void ReassignCoordinates()
    {
        for (int i = 0; i < RoomMap.Count; i++)
        {
            for (int j = 0; j < RoomMap[i].Count; j++)
            {
                RoomMap[i][j].Match(room => room.coordinates = new Vector2Int(j, i), () => { });
            }
        }
    }

    private void StrechConnections()
    {
        for (int yRoom = 0; yRoom < RoomMap.Count; yRoom++)
        {
            for (int xRoom = 0; xRoom < RoomMap[yRoom].Count; xRoom++)
            {
                RoomMap[yRoom][xRoom].Match(r =>
                {
                    foreach (var r2 in r.connections)
                    {
                        if (r.coordinates.x < r2.coordinates.x && r2.coordinates.x - r.coordinates.x > 1)
                        {
                            // Go from left to right and add horizontal connections
                            for (int x = r.coordinates.x + 1; x < r2.coordinates.x; x++)
                            {
                                RoomMap[r.coordinates.y][x] = Optional<DungeonRoomViewData>.Some(new DungeonRoomViewData(true, false));
                            }
                        }
                        
                        if (r.coordinates.y < r2.coordinates.y && r2.coordinates.y - r.coordinates.y > 1)
                        {
                            // Go from bottom to top and add vertical connections
                            for (int y = r.coordinates.y + 1; y < r2.coordinates.y; y++)
                            {
                                RoomMap[y][r.coordinates.x] = Optional<DungeonRoomViewData>.Some(new DungeonRoomViewData(false, true));
                            }
                        }
                    }
                }, () => { });
            }
        }
    }
    
    public class DungeonRoomViewData
    {
        public DungeonRoomViewData(string name, Vector2Int coordinates)
        {
            // Draw an actual room
            this.name = name;
            this.coordinates = coordinates;

            hasRoom = true;
            connections = new List<DungeonRoomViewData>();
        }

        public DungeonRoomViewData(bool passedHorizontally, bool passedVertically)
        {
            // Only draw connections to other rooms
            this.passedHorizontally = passedHorizontally;
            this.passedVertically = passedVertically;

            hasRoom = false;
            connections = new List<DungeonRoomViewData>();
        }

        public bool HasLeftConnection()
        {
            return
                passedHorizontally ||
                connections.Exists(r => r.coordinates.y == coordinates.y && r.coordinates.x < coordinates.x);
        }
        
        public bool HasRightConnection()
        {
            return
                passedHorizontally ||
                connections.Exists(r => r.coordinates.y == coordinates.y && r.coordinates.x > coordinates.x);
        }
        
        public bool HasTopConnection()
        {
            return
                passedVertically ||
                connections.Exists(r => r.coordinates.x == coordinates.x && r.coordinates.y < coordinates.y);
        }
        
        public bool HasBottomConnection()
        {
            return
                passedVertically ||
                connections.Exists(r => r.coordinates.x == coordinates.x && r.coordinates.y > coordinates.y);
        }
        
        public readonly List<DungeonRoomViewData> connections;
        public readonly string name;
        private readonly bool passedVertically;
        private readonly bool passedHorizontally;
        public readonly bool hasRoom;
        public Vector2Int coordinates;
    }
}