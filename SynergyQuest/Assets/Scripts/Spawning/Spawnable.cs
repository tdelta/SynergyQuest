using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

/**
 * <summary>
 * Allows an object to respawn, for example a player.
 * Respawning means that the object will change its position to a respawn point etc.
 * 
 * Furthermore, other behaviours can listen to the <see cref="OnRespawn"/> event for example to restore the health of
 * a player etc.
 *
 * The default respawn point must be set as the property <see cref="defaultRespawnPosition"/>.
 * Usually this role is fulfilled by <see cref="PlayerSpawner"/>.
 *
 * However, sometimes it may be necessary to provide a custom respawn point very specific to the current situation.
 * For example, when a player falls down a <see cref="Chasm"/>, they should be respawned at the point where they entered
 * the chasm.
 * This can be done by registering a function which provides such points using <see cref="AddRespawnPointProvider"/>.
 * If the special situation no longer applies, such functions can be unregistered using
 * <see cref="RemoveRespawnPointProvider"/>.
 * Is multiple such functions are registered, the one who registered the latest will be used.
 * </summary>
 * <seealso cref="PlayerSpawner"/>
 */
public class Spawnable : MonoBehaviour
{
    [NonSerialized] [CanBeNull] public Transform defaultRespawnPosition;
    
    /**
     * <summary>
     * Event that is invoked after this object has been respawned by a call to <see cref="Respawn"/>.
     * </summary>
     */
    public event OnRespawnAction OnRespawn;
    public delegate void OnRespawnAction(Vector3 respawnPosition);
    
    private List<Func<Vector3>> _respawnPointProviders = new List<Func<Vector3>>();

    /**
     * <summary>
     * Lets this object respawn.
     * 
     * The first position available from the following list will be selected as the respawn position:
     * <list type="bullet">
     *   <item><description>custom respawn point provider (see class description)</description></item>
     *   <item><description>default respawn position</description></item>
     *   <item><description>current position of this object</description></item>
     * </list>
     *
     * See the class description for further explanation.
     * This method will also make this object visible using <see cref="GameObjectExtensions.MakeVisible"/>.
     * </summary>
     */
    public void Respawn()
    {
        var respawnPosition = DetermineCustomRespawnPosition()
            .Else(
                defaultRespawnPosition != null ?
                    // ReSharper disable once PossibleNullReferenceException
                    Optional<Vector3>.Some(defaultRespawnPosition.position) :
                    DebugExtensions.LogErrorWithValue(
                            "Respawned without having a default spawn point. Continuing on same position, but this should never happen.", 
                            Optional<Vector3>.None()
                    )
            )
            .ValueOr(this.transform.position);

        // Move to position of the spawner when respawning
        if (TryGetComponent(out PhysicsEffects physicsEffects))
        {
            physicsEffects.Teleport(respawnPosition);
        }

        else if (TryGetComponent(out Rigidbody2D body))
        {
            body.position = respawnPosition;
        }

        else
        {
            this.transform.position = respawnPosition;
        }

        // Make visible again(, if we have been invisible)
        this.gameObject.MakeVisible();
        
        OnRespawn?.Invoke(respawnPosition);
    }
    
    /**
     * Register a custom function to provide respawn points, see class description.
     */
    public void AddRespawnPointProvider(Func<Vector3> provider)
    {
        if (_respawnPointProviders.Contains(provider))
        {
            _respawnPointProviders.Remove(provider);
        }
        
        _respawnPointProviders.Add(provider);
    }

    
    /**
     * Unregister a custom function providing respawn points, see class description.
     */
    public void RemoveRespawnPointProvider(Func<Vector3> provider)
    {
        _respawnPointProviders.Remove(provider);
    }

    /**
     * <summary>
     * If custom respawn point providers have been registered, it returns a respawn point produced by the one which
     * registered last. Otherwise it returns <see cref="Optional{T}.None"/>.
     *
     * See class description for further explanation.
     * </summary>
     */
    private Optional<Vector3> DetermineCustomRespawnPosition()
    {
        return Optional<Func<Vector3>>
            .FromNullable(_respawnPointProviders.LastOrDefault())
            .Map(provider => provider());
    }
}
