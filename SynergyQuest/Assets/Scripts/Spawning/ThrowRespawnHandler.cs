using UnityEngine;

/**
 * <summary>
 * Ensures a <see cref="Spawnable"/> is respawned at the position of the player who picked it up last, if it is also a
 * <see cref="Throwable"/> and died shortly after landing after a throw.
 * </summary>
 */
[RequireComponent(typeof(Spawnable))]
[RequireComponent(typeof(Throwable))]
public class ThrowRespawnHandler : MonoBehaviour
{
    private Spawnable _spawnable;
    private Throwable _throwable;

    /**
     * Caches the last player, who picked up this throwable object
     */
    private PlayerController _carrier = null;
    
    /**
     * <summary>
     * Provides the position of the player who last picked up this object as respawn point to <see cref="Throwable"/>
     * while this object is being thrown
     * </summary>
     */
    private Vector3 ProvideRespawnPoint()
    {
        return _carrier.transform.position;
    }

    private void Awake()
    {
        _spawnable = GetComponent<Spawnable>();
        _throwable = GetComponent<Throwable>();
    }

    private void OnEnable()
    {
        _throwable.OnPickedUp += OnPickedUp;
        _throwable.OnLanded += OnLanded;
    }

    private void OnDisable()
    {
        _throwable.OnPickedUp -= OnPickedUp;
        _throwable.OnLanded -= OnLanded;
    }

    /**
     * <summary>
     * Called when this object is being picked up to throw.
     * Caches the player picking it up and provides their position to <see cref="Spawnable"/> as respawn point.
     * </summary>
     * <seealso cref="Throwable.OnPickedUp"/>
     * <seealso cref="Spawnable.AddRespawnPointProvider"/>
     */
    void OnPickedUp(PlayerController carrier)
    {
        _carrier = carrier;
        _spawnable.AddRespawnPointProvider(
            ProvideRespawnPoint,
            1 // over-rule Chasm as respawn point provider
        );
    }
    
    /**
     * <summary>
     * Called when this object has landed after being thrown.
     * Removes this behaviour from its role as respawn point provider from <see cref="Spawnable"/>.
     * </summary>
     * <seealso cref="Throwable.OnLanded"/>
     * <seealso cref="Spawnable.RemoveRespawnPointProvider"/>
     */
    void OnLanded()
    {
        // Give the physics time to kill the player...
        StartCoroutine(
            CoroutineUtils.WaitUntilPhysicsStepCompleted(() =>
            {
                // ...and stop being a respawn point provider, after a phyisics step has been performed
                _spawnable.RemoveRespawnPointProvider(ProvideRespawnPoint, 1);
            })
        );
    }
}
