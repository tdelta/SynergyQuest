using UnityEngine;

/**
 * Utility behavior which allows to determine whether a player is interacting with an object by pressing some button.
 * The player must be touching the object.
 *
 * Other behaviors can then access the `IsInteracting` property of this class to determine, whether a player is
 * interacting.
 * See for example the `DeadManSwitch` behavior.
 *
 * A Collider Component must be present.
 */
public class Interactive : MonoBehaviour
{
    /**
     * 3 types of interactions are supported:
     *
     * - Pressing a button once
     * - Releasing a button
     * - Holding a button
     */
    public enum InteractionType
    {
        Down,
        Up,
        Hold
    }

    /**
     * Which button must a player press to interact?
     */
    [SerializeField] private Button button;
    /**
     * The kind of interaction that is registered, see enum above.
     */
    [SerializeField] private InteractionType interactionType;

    /**
     * The player which is currently touching this object and who can interact.
     * If multiple players are touching the object, only the first one can interact.
     *
     * FIXME: We might want to allow multiple players to interact?
     */
    private PlayerController _interactingPlayer = null;
    
    /**
     * Whether a player is triggering an interaction. See also class description.
     */
    private bool _isInteracting = false;
    public bool IsInteracting => _isInteracting;

    private void Update()
    {
        // If a player is currently touching this object...
        if (!ReferenceEquals(_interactingPlayer, null))
        {
            // Determine whether the player is pressing the interaction button in the right way:
            switch (interactionType)
            {
                case InteractionType.Down:
                    _isInteracting = _interactingPlayer.Input.GetButtonDown(button);
                    break;
                case InteractionType.Up:
                    _isInteracting = _interactingPlayer.Input.GetButtonUp(button);
                    break;
                case InteractionType.Hold:
                    _isInteracting = _interactingPlayer.Input.GetButton(button);
                    break;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // If there is not already a player touching this object,
        // and if the other object touching this object is a player,
        // remember the object as the player who is potentially interacting with this object.
        //
        // See also the `Update` method
        if (ReferenceEquals(_interactingPlayer, null) && other.gameObject.CompareTag("Player"))
        {
            _interactingPlayer = other.gameObject.GetComponent<PlayerController>();
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        // If there has been a player touching this object and the object exiting the collision is this player...
        if (!ReferenceEquals(_interactingPlayer, null) && _interactingPlayer.gameObject == other.gameObject)
        {
            // ...no longer remember that player
            _interactingPlayer = null;
            // ...and interrupt any interaction that might have taken place
            _isInteracting = false;
        }
    }
}
