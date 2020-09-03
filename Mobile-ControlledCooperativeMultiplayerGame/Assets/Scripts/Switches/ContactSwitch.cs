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
