using UnityEngine;

public class ContactAttack : MonoBehaviour
{
    [SerializeField] private GameObject customAttacker = default;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Attackable attackable))
        {
            var attackPoint = other.ClosestPoint(this.transform.position);
            var attackDirection = (Vector2) other.transform.position - attackPoint;

            attackable.Attack(
                // ReSharper disable once Unity.NoNullCoalescing
                customAttacker ?? this.gameObject,
                Optional<Vector2>.Some(attackDirection)
            );
        }
    }
}
