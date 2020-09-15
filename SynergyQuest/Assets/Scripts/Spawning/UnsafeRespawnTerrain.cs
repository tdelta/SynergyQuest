using UnityEngine;

/**
 * <summary>
 * Marks an object as terrain which is unsafe for respawning, see <see cref="Spawnable"/>.
 * This marking is registered at any <see cref="Spawnable"/> touching this object, given both, the spawnable, and this
 * object have colliders.
 * </summary>
 */
public class UnsafeRespawnTerrain : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Spawnable>(out var spawnable))
        {
            spawnable.RegisterTouchingUnsafeTerrain(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Spawnable>(out var spawnable))
        {
            spawnable.UnregisterTouchingUnsafeTerrain(this);
        }
    }
}
