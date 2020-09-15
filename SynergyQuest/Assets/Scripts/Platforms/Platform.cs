using UnityEngine;

/**
 * <summary>
 * This behavior is intended for (moving) platforms on top of <see cref="Chasm"/>s.
 * It ensures that other objects implementing <see cref="PlatformTransportable"/> and <see cref="PhysicsEffects"/> are
 * moved along with this object.
 *
 * To actually move a platform, additionally use <see cref="WaypointControlledPlatform"/> or
 * <see cref="PlayerControlledPlatform"/>.
 * </summary>
 */
public class Platform : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only transport an object, if it has a `PlatformTransportable` component
        if (other.TryGetComponent(out PlatformTransportable _))
        {
            // If the object has a physics effects component, we set the platform as new origin which it from now on
            // shall move relative to
            if (other.TryGetComponent(out PhysicsEffects physicsEffects))
            {
                physicsEffects.SetCustomOrigin(this.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Only handle objects, if they have a `PlatformTransportable` component
        if (other.TryGetComponent(out PlatformTransportable _))
        {
            // If the object has a physics effects component...
            if (other.TryGetComponent(out PhysicsEffects physicsEffects))
            {
                // ...and it is using this platform as custom origin to move relative to, then remove the custom origin,
                // since the object has left the platform
                if (physicsEffects.CustomOrigin == this.transform)
                {
                    physicsEffects.RemoveCustomOrigin();
                }
            }
        }
    }
}
