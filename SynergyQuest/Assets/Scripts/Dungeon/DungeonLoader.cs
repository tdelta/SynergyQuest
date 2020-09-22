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
 * Behavior which tries to load a dungon layout on Awake, if none is loaded already.
 * It does not reload the current scene though, as it assumes the current scene is already a dungeon room.
 *
 * The main purpose of this behavior is to load a dungeon layout while debugging/developing a room scene in the Unity
 * editor.
 */
public class DungeonLoader: MonoBehaviour
{
    /**
     * Name of the dungeon room the current scene is representing
     */
    [SerializeField] private string room = default;
    /**
     * Path to the JSON layout file describing the dungeon
     */
    [SerializeField] private string filePath = default;
    /**
     * Number of players for which the dungeon will be loaded.
     * If set to -1, `DebugSettings.Instance.DefaultDungeonPlayerNum` will be used instead.
     */
    [SerializeField] private int playerNum = -1;
        
    void Awake()
    {
        // If no dungeon layout has been loaded yet...
        if (!DungeonLayout.Instance.IsLoaded)
        {
            // Determine the number of players for which the dungeon shall be loaded
            // (see field description of `playerNum`)
            var playerNum = this.playerNum == -1 ?
                  DebugSettings.Instance.DefaultDungeonPlayerNum
                : this.playerNum;
            
            // Load the layout file of the dungeon
            DungeonLayout.Instance.LoadDungeon(
                filePath,
                playerNum,
                room,
                true // as we assume the current scene is already representing the given room, we do not
                                   // want to reload it.
            );
        }
    }
}
