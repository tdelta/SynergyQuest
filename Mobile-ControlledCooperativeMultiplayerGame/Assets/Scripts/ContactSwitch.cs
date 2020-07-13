using UnityEngine;
using UnityEngine.Events;

public class ContactSwitch : MonoBehaviour
{
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
          if (other.CompareTag("Player"))
          {
               onContactEvent?.Invoke();
          }
     }
}
