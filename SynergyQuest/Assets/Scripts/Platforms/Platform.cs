using UnityEngine;

/**
 * <summary>
 * This behavior is intended for (moving) platforms on top of <see cref="Chasm"/>s.
 * It ensures that other objects implementing <see cref="PlatformTransportable"/> and <see cref="PhysicsEffects"/> are
 * moved along with this object.
 *
 * To actually move a platform, additionally use <see cref="WaypointControlledPlatform"/> or
 * <see cref="PlayerControlledPlatform"/>.
 *
 * Players who die on the platform or drop into the chasm can be respawned on the platform, if
 * <see cref="respawnPoint"/> is set. See also <see cref="Spawnable"/> and <see cref="ChasmContactTracker"/>.
 * </summary>
 */
public class Platform : MonoBehaviour
{
    /**
     * If set, players dying on the platform or by falling into the chasm will be respawned at this point.
     */
    [SerializeField] private Transform respawnPoint = null;

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

            // If a respawn point has been set...
            if (respawnPoint != null)
            {
                // ...and if the other object is a Spawnable...
                if (other.TryGetComponent(out Spawnable spawnable))
                {
                    // ...register this behaviour to provide a respawn point to it, as long as it is in contact with the
                    // platform
                    spawnable.AddRespawnPointProvider(ProvideRespawnPoint);
                }
                
                // Also, if the other object has a ChasmContactTracker, we inform it, that if it object falls down the
                // chasm it shall respawn on this platform, even if it lost contact to the platform at this point.
                if (other.TryGetComponent(out ChasmContactTracker chasmTracker))
                {
                    chasmTracker.SetRespawningPlatform(this);
                }
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

            // If a respawn point has been set...
            if (respawnPoint != null)
            {
                // ...and if the other object is a Spawnable...
                if (other.TryGetComponent(out Spawnable spawnable))
                {
                    // ...unregister this behaviour to provide a respawn point as the spawnable is no longer in contact
                    // with this platform
                    spawnable.RemoveRespawnPointProvider(ProvideRespawnPoint);
                }
            }
        }
    }
    
    /**
     * Used with <see cref="Spawnable.AddRespawnPointProvider"/> to let this behaviour define
     * a custom respawn point.
     */
    Vector3 ProvideRespawnPoint()
    {
        return respawnPoint.transform.position;
    }
}
