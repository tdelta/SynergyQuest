using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Allows an object to respawn, for example a player.
 * Respawning means that the object will change its position to a respawn point etc.
 * </summary>
 * <remarks>
 * Furthermore, other behaviours can listen to the <see cref="OnRespawn"/> event for example to restore the health of
 * a player etc.
 *
 * It constantly saves the current object position as respawn position, unless this object is on unsafe terrain
 * (terrain where nothing should be respawned, e.g. chasms).
 * Other behaviours can mark unsafe terrain using <see cref="RegisterTouchingUnsafeTerrain"/> and
 * <see cref="UnregisterTouchingUnsafeTerrain"/>. See also <see cref="UnsafeRespawnTerrain"/>.
 * </remarks>
 */
public class Spawnable : MonoBehaviour
{
    /**
     * Last safe respawn position.
     * See class description for explanation.
     */
    public Vector3 RespawnPosition { get; private set; } = Vector3.zero;
    
    /**
     * All objects which have registered as unsafe terrain currently touching this object.
     */
    private HashSet<object> _touchingUnsafeTerrains = new HashSet<object>();
    /**
     * This object is on unsafe terrain iff <see cref="_touchingUnsafeTerrain"/> is not empty
     */
    private bool IsOnUnsafeTerrain => _touchingUnsafeTerrains.Any();
    
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

    /**
     * <summary>
     * Registers, that this object is currently touching <see cref="obj"/> which is terrain not safe for respawning.
     * </summary>
     * <remarks>
     * As long as there is an unsafe terrain in contact, this behaviour will not save the current position as respawn
     * position.
     * </remarks>
     * <seealso cref="UnregisterTouchingUnsafeTerrain"/>
     */
    public void RegisterTouchingUnsafeTerrain(object obj)
    {
        _touchingUnsafeTerrains.Add(obj);
    }

    /**
     * <summary>
     * Registers, that this object is no longer touching <see cref="obj"/> which is terrain not safe for respawning.
     * </summary>
     * <remarks>
     * As long as there is an unsafe terrain in contact, this behaviour will not save the current position as respawn
     * position.
     * </remarks>
     * <seealso cref="RegisterTouchingUnsafeTerrain"/>
     */
    public void UnregisterTouchingUnsafeTerrain(object obj)
    {
        _touchingUnsafeTerrains.Remove(obj);
    }

    /**
     * <summary>
     * This method is repeatedly called to save the current object position as respawn position.
     * It does nothing, if <see cref="IsOnUnsafeTerrain"/> is true.
     * </summary>
     */
    private void SaveRespawnPosition()
    {
        if (!IsOnUnsafeTerrain)
        {
            RespawnPosition = this.transform.position;
        }
    }

    private void Start()
    {
        SaveRespawnPosition();
        InvokeRepeating(nameof(SaveRespawnPosition), 0.0f, 0.1f);
    }

    /**
     * <summary>
     * Lets this object respawn.
     * </summary>
     * <remarks>
     * See the class description for an explanation on how respawn positions are determined.
     * This method will also make this object visible using <see cref="GameObjectExtensions.MakeVisible"/>.
     * </remarks>
     */
    public void Respawn(RespawnReason reason = RespawnReason.Other)
    {
        var respawnPosition = RespawnPosition;
        
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
}