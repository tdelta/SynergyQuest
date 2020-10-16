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
 * If players use a door to enter a room of a dungeon layout, they will be spawned by this spawner.
 * For this to work, an instance of this behavior should be associated with every door as a child object and placed
 * correctly, as players will be spawned at its location.
 */
public class DoorSpawner: PlayerSpawner
{
    /**
     * Set this field to the door this spawner belongs to
     */
    [SerializeField] private Door door = default;

    protected override bool IsSpawnerActive()
    {
        // The spawner should only be active, if it belongs to the door which has been used to enter the room
        return DungeonLayout.Instance.SpawnerDoorId == door.DoorId;
    }

    protected override void OnSpawn(PlayerController player)
    {
        // Let players face the right direction after passing through this door
        player.FaceDirection(PlayerDataKeeper.Instance.LastDoorDirection);
    }
}
