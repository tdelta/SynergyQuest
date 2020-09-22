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
using UnityEngine;
using UnityEngine.Events;

/**
 * <summary>
 * Switch which is triggered, as soon as a player comes in contact with this object.
 * See <see cref="Switch"/> and <see cref="Switchable"/> on usage information.
 * </summary>
 */
[RequireComponent(typeof(Switch))]
public class ContactSwitch : MonoBehaviour
{
     private Switch _switch;

     private PlayerController _playerInContact = null;

     private void Awake()
     {
          _switch = GetComponent<Switch>();
     }

     private void OnTriggerEnter2D(Collider2D other)
     {
          HandleContact(other, false);
     }

     private void OnCollisionEnter2D(Collision2D other)
     {
          HandleContact(other.collider, false);
     }
     
     private void OnTriggerExit2D(Collider2D other)
     {
          HandleContact(other, true);
     }

     private void OnCollisionExit2D(Collision2D other)
     {
          HandleContact(other.collider, true);
     }

     void HandleContact(Collider2D other, bool loosingContact)
     {
          // If no player is already triggering the switch and contact was established by a player...
          if (
               !loosingContact &&
               ReferenceEquals(_playerInContact, null) &&
               other.CompareTag("Player")
          )
          {
               // ...trigger the switch and remember the player, so that we can recognize them when they loose contact
               _playerInContact = other.GetComponent<PlayerController>();
               _switch.Value = true;
          }
          
          // Else if a player is already triggering the switch and the game object of this player looses contact with this object..
          else if (
               loosingContact &&
               !ReferenceEquals(_playerInContact, null) &&
               other.gameObject == _playerInContact.gameObject
          )
          {
               // ...reset the switch and forget the player
               _playerInContact = null;
               _switch.Value = false;
          }
     }
}
