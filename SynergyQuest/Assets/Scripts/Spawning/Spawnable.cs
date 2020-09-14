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
 * A default respawn point can be set as the property <see cref="defaultRespawnPosition"/>.
 * Usually this role is fulfilled by <see cref="PlayerSpawner"/>.
 *
 * However, sometimes it may be necessary to provide a custom respawn point very specific to the current situation.
 * For example, when a player falls down a <see cref="Chasm"/>, they should be respawned at the point where they entered
 * the chasm.
 * This can be done by registering a function which provides such points using <see cref="AddRespawnPointProvider"/>.
 * If the special situation no longer applies, such functions can be unregistered using
 * <see cref="RemoveRespawnPointProvider"/>.
 * Is multiple such functions are registered, the one who registered the latest will be used.
 *
 * If none of the above methods are used to determine a respawn point, respawn will be performed on the position this
 * object is currently at, without changing it.
 * </summary>
 * <seealso cref="PlayerSpawner"/>
 */
public class Spawnable : MonoBehaviour
{
    [NonSerialized] [CanBeNull] public Transform defaultRespawnPosition;

    /**
     * Encodes, why an object is respawned
     */
    public enum RespawnReason
    {
        Death,
        Other
    }
    
    /**
     * <summary>
     * Event that is invoked after this object has been respawned by a call to <see cref="Respawn"/>.
     * </summary>
     */
    public event OnRespawnAction OnRespawn;
    public delegate void OnRespawnAction(Vector3 respawnPosition, RespawnReason reason);
    
    private SortedDictionary<int, List<Func<Vector3>>> _respawnPointProviders = new SortedDictionary<int, List<Func<Vector3>>>();

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
    public void Respawn(RespawnReason reason = RespawnReason.Other)
    {
        var respawnPosition = DetermineCurrentRespawnPosition();

        RespawnAt(respawnPosition, reason);
    }

    /**
     * <summary>
     * Same as <see cref="Respawn"/> but respawns at the given position.
     * </summary>
     */
    public void RespawnAt(Vector3 respawnPosition, RespawnReason reason = RespawnReason.Other)
    {
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
        
        OnRespawn?.Invoke(respawnPosition, reason);
    }
    
    /**
     * <summary>
     * Register a custom function to provide respawn points, see class description.
     * </summary>
     * <param name="priority">
     *     Providers with higher priority are selected first, even if a provider with a lower
     *     priority has been added at a later point in time.
     * </param>
     */
    public void AddRespawnPointProvider(Func<Vector3> provider, int priority = 0)
    {
        if (_respawnPointProviders.TryGetValue(priority, out var list))
        {
            if (!list.Contains(provider))
            {
                list.Add(provider);
            }
        }

        else
        {
            _respawnPointProviders.Add(priority, new List<Func<Vector3>> {provider});
        }
    }

    
    /**
     * <summary>
     * Unregister a custom function providing respawn points, see class description.
     * </summary>
     * <param name="priority">
     *     this parameter must match the value provided when calling <see cref="AddRespawnPointProvider"/>.
     * </param>
     */
    public void RemoveRespawnPointProvider(Func<Vector3> provider, int priority = 0)
    {
        if (_respawnPointProviders.TryGetValue(priority, out var list))
        {
            list.Remove(provider);
        }
    }

    /**
     * <summary>
     * If custom respawn point providers have been registered, it returns the one provider with the highest priority who
     * registered last. Otherwise it returns <see cref="Optional{T}.None"/>
     *
     * See class description for further explanation.
     * </summary>
     */
    public Optional<Func<Vector3>> DetermineCurrentRespawnProvider()
    {
        return Optional<Func<Vector3>>
            .FromNullable(_respawnPointProviders.LastOrDefault().Value?.LastOrDefault());
    }

    /**
     * <summary>
     * Returns the current respawn position of this <see cref="Spawnable"/> as determined by the rules outlined in the
     * class description.
     * </summary>
     */
    public Vector3 DetermineCurrentRespawnPosition()
    {
        return DetermineCurrentRespawnProvider()
            .Map(provider => provider())
            .Else(
                defaultRespawnPosition != null ?
                    // ReSharper disable once PossibleNullReferenceException
                    Optional<Vector3>.Some(defaultRespawnPosition.position) :
                    Optional<Vector3>.None()
            )
            .ValueOr(this.transform.position);
    }
}