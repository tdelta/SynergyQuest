using UnityEngine;

/**
 * Players have a child object with this behavior.
 * 
 * This child object has an additional trigger collider which is used by
 * the `Interactive` behavior to detect, whether the player is touching
 * an object.
 * This was necessary, since the rigidbody and collider of the player
 * get deactivated in some situations (e.g. by `MovementBinder`) but
 * we still want to register interactions.
 *
 * The purpose of this behavior is just to provide a reference to the
 * main player object.
 */
public class InteractorCollider : MonoBehaviour
{
    [SerializeField] private PlayerController player = default;
    public PlayerController Player => player;
    
    /**
     * <summary>
     * If false, this player will not be able to interact with an <see cref="Interactive"/> object
     * </summary>
     */
    public bool CanInteract = true;
}
