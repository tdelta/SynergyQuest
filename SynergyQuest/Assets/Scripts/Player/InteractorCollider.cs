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
 * Players have a child object with this behavior.
 * 
 * This child object has an additional trigger collider which is used by
 * the `Interactive` behavior to detect, whether the player is touching
 * an object.
 * This was necessary, since the rigidbody and collider of the player
 * get deactivated in some situations but we still want to register
 * interactions.
 *
 * The purpose of this behavior is just to provide a reference to the
 * main player object.
 */
public class InteractorCollider : MonoBehaviour
{
    [SerializeField] private PlayerController player = default;
    public PlayerController Player => player;
    
    /**
     * <summary>
     * If false, this player will not be able to interact with an <see cref="Interactive"/> object
     * </summary>
     */
    public bool CanInteract = true;
}
