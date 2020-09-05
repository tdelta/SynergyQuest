using System;
using UnityEngine;

/**
 * <summary>
 * Keeps track of where a <see cref="Spawnable"/> object has entered a <see cref="Chasm"/> first so that it can let the
 * object respawn at that position when it falls down the chasm.
 *
 * It also allows platforms which support respawning on them to register themselves here using
 * <see cref="SetRespawningPlatform"/>. This way, this object can respawn on such a platform.
 * </summary>
 */
[RequireComponent(typeof(Spawnable))]
public class ChasmContactTracker : MonoBehaviour
{
    /**
     * <summary>
     * Where this object first entered a <see cref="Chasm"/>. It is set to <see cref="Optional{T}.None"/> when this
     * object is not on a chasm.
     * </summary>
     */
    public Optional<Vector3> ChasmEntryPoint { get; private set; }
    public bool IsOnChasm => ChasmEntryPoint.IsSome();

    /**
     * <summary>
     * While true, this component will not register, whether this object enters or leaves chasms.
     * </summary>
     */
    [NonSerialized] public bool Paused = false;

    private Collider2D _collider;
    private Spawnable _spawnable;

    private Func<Vector3> _respawnPointProvider;
    /**
     * <see cref="SetRespawningPlatform"/>
     */
    private Platform _lastRespawningPlatform = null;
    
    /**
     * A <see cref="Platform"/> can call this method to indicate that a player crossing a <see cref="Chasm"/> on it
     * shall be respawned on this platform. If set, this behaviour will use the position of the platform to respawn
     * players instead of their last position before they entered the chasm.
     */
    public void SetRespawningPlatform(Platform respawningPlatform)
    {
        if (IsOnChasm)
        {
            _lastRespawningPlatform = respawningPlatform;
        }
    }
    
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _spawnable = GetComponent<Spawnable>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Paused)
        {
            // If we collided with a chasm...
            if (other.CompareTag("Chasm"))
            {
                // And we do not already have a first entry point to a chasm area...
                if (ChasmEntryPoint.IsNone())
                {
                    // We remember the current position as the first entry point to the chasm area
                    ChasmEntryPoint = Optional<Vector3>.Some(this.transform.position);

                    // Register ourselves at the Spawnable component to provide a custom respawn point
                    _spawnable.AddRespawnPointProvider(ProvideRespawnPoint);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!Paused)
        {
            // If we are no longer in contact with any chasm...
            if (other.CompareTag("Chasm") && !Chasm.IsOnChasm(_collider))
            {
                // Forget the first entry point to the chasm we have been in
                ChasmEntryPoint = Optional<Vector3>.None();
                // ...and any platforms which might have registered to act as respawn point (see SetRespawningPlatform)
                _lastRespawningPlatform = null;

                // Unregister ourselves at the Spawnable component as a provider of custom respawn points
                _spawnable.RemoveRespawnPointProvider(ProvideRespawnPoint);
            }
        }
    }
    
    /**
     * Used with <see cref="Spawnable.AddRespawnPointProvider"/> to let this behaviour define
     * a custom respawn point.
     */
    Vector3 ProvideRespawnPoint()
    {
        // If a platform registered here as a respawn point (see class description) use its position as respawn point
        if (!ReferenceEquals(_lastRespawningPlatform, null))
        {
            return _lastRespawningPlatform.transform.position;
        }

        // Otherwise use the position where we first entered a chasm as respawn point
        else
        {
            return ChasmEntryPoint.ValueOr(Vector3.zero);
        }
    }
}
