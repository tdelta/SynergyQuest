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
 * Behavior which provides an event which one can subscribe to in the Unity inspector.
 * The event is triggered, as soon as a player comes in contact with this object.
 *
 * DEPRECATED: Use <see cref="ContactSwitch"/> instead.
 * </summary>
 */
[Obsolete("Use ContactSwitch instead.")]
public class ContactTrigger : MonoBehaviour
{
     // Event mentioned in the description above
     [SerializeField] private ContactEvent onContactEvent = default;

     private Collider2D _collider;

     private void Awake()
     {
          _collider = GetComponent<Collider2D>();
     }

     private void OnTriggerEnter2D(Collider2D other)
     {
          HandleContact(other);
     }

     private void OnCollisionEnter2D(Collision2D other)
     {
          HandleContact(other.collider);
     }

     void HandleContact(Collider2D other)
     {
          // If the object which is touching this one is a player, invoke the event.
          if (other.CompareTag("Player"))
          {
               onContactEvent?.Invoke(other.GetComponent<PlayerController>());
          }
     }

     /**
      * Fires the `onContactEvent` for all players currently in contact with this object.
      */
     public void RecheckContacts()
     {
          var contacts = new List<Collider2D>();
          _collider.GetContacts(contacts);
          
          contacts.ForEach(HandleContact);
     }
}

[Serializable]
class ContactEvent : UnityEvent<PlayerController>
{ }
