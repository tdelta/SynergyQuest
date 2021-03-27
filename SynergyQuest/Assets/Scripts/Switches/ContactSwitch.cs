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
using Utils;

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

     [Tooltip("Only trigger this switch, if the player who is in contact also actively walks into the direction of this object.")]
     [SerializeField] private bool playerMustSteerInto = false;
     [Tooltip("Only trigger, if the contact persists at least for this amount of seconds.")]
     [SerializeField] private float activationTimeout = 0.0f;

     // Records the time when the contact with a player started.
     // It is needed to ascertain, whether the activationTimeout elapsed or not
     private float _contactStartTime = float.NaN;

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

     private void FixedUpdate()
     {
          // Repeatedly check, if the activationTimeout elapsed
          CheckTimeout();
     }

     /**
      * If a player is currently in contact, check if it has been in contact for at least <see cref="activationTimeout"/>
      * seconds.
      * Only then change the activation of the switch.
      */
     private void CheckTimeout()
     {
          // We only need to check for a timeout, if there is a player currently in contact
          if (_playerInContact.IsNotNull())
          {
               // Check if _contactStartTime is set.
               // If not, the timeout has already been completed
               if ( !float.IsNaN(_contactStartTime) )
               {
                    // Either we dont require the player to actively walk into the direction of this object to activate
                    // this switch,
                    // OR the player must be actively walking into the direction of this object
                    if (
                         !playerMustSteerInto ||
                         _playerInContact.PhysicsEffects.SteeringDirection.IsSameDirectionAs(
                              this.transform.position - _playerInContact.transform.position
                         )
                    )
                    {
                         // If the timeout completed, activate the switch
                         if ( Time.time - _contactStartTime >= activationTimeout )
                         {
                              _contactStartTime = float.NaN;
                              _switch.Value = true;
                         }
                    }

                    // If the steering conditions have been violated, reset the timeout
                    else
                    {
                         _contactStartTime = Time.time;
                    }
               }
          }
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
               // ...remember the player, so that we can recognize them when they loose contact
               _playerInContact = other.GetComponent<PlayerController>();
               // ...and start the timeout required until the switch activates, see activationTimeout field
               _contactStartTime = Time.time;
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
