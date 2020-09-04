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
