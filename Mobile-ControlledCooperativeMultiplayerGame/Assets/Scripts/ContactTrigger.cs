using UnityEngine;
using UnityEngine.Events;

/**
 * Behavior which provides an event which one can subscribe to in the Unity inspector.
 * The event is triggered, as soon as a player comes in contact with this object.
 */
public class ContactTrigger : MonoBehaviour
{
     // Event mentioned in the description above
     [SerializeField] private UnityEvent onContactEvent;

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
               onContactEvent?.Invoke();
          }
     }
}