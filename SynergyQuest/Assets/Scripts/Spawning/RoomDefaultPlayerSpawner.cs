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

﻿/**
 * Spawns player prefab instances at its current position, if they have not entered the scene through a door.
 * 
 * This includes locally controlled debugging instances and remotely controlled instances.
 * It also respawns dead players and already existing players from a previous scene, if scenes have been switched.
 *
 * This behavior should be used for entry points to a dungeon, or to spawn players in intermediate dungeon rooms for
 * debugging purposes.
 * 
 * See the superclass for more information.
 */
public class RoomDefaultPlayerSpawner : PlayerSpawner
{
    protected override bool IsSpawnerActive()
    {
        // This spawner is only active if a players have not entered the current dungeon room through a door.
        return !DungeonLayout.Instance.IsUsingDoorAsSpawner;
    }
}
