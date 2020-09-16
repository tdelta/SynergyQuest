using System;
using UnityEngine;
using UnityEngine.Events;

/**
 * Utility behavior which allows to determine whether a player is interacting with an object by pressing some button.
 * The player must be touching the object and be looking at it. They must also have the correct color, if the Color
 * property of this behaviour is not set to `Any`.
 *
 * Other behaviors can then access the `IsInteracting` property of this class to determine, whether a player is
 * interacting.
 * See for example the `DeadManSwitch` behavior.
 *
 * Alternatively, one can subscribe to the `interactionTriggeredEvent`.
 *
 * A Collider Component must be present.
 *
 * This behavior also causes players to display a speech bubble, giving a hint about possible interactions
 * (see also the `InteractionSpeechBubble` behavior).
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
    [SerializeField] private Button button = default;
    public Button Button => button;
    
    /**
     * The kind of interaction that is registered, see enum above.
     */
    [SerializeField] private InteractionType interactionType = default;

    /**
     * If true, the button of this interactive should only be enabled, while the player is colliding with it.
     * If false, the interactive is only used to display a speech bubble
     */
    [SerializeField] private bool AffectsController = true;
    
    /**
      * Whether this Interactive shall temporarily ignore collision logic,
      * i.e. if there is currently a player interacting with this object
      * and this is set, then the player will still be able to interact, even
      * if they stop colliding with this object.
      */
    public bool IgnoreCollisions { get; set; } = false;


     /**
     * The player which is currently touching this object and who can interact.
     * If multiple players are touching the object, only the first one can interact.
     *
     * FIXME: We might want to allow multiple players to interact?
     */
    private PlayerController _interactingPlayer = null;
    public PlayerController InteractingPlayer {
        get => _interactingPlayer;
        private set {
            if (!ReferenceEquals(value, _interactingPlayer))
            {
                if (ReferenceEquals(value, null) && AffectsController) {
                    // We stopped interacting with something, hence
                    // we must disable the corresponding interaction on
                    // the controllers:
                    _interactingPlayer.Input.EnableButtons((button, false));
                } else if (AffectsController){
                    if (interactionType == InteractionType.Down || interactionType == InteractionType.Hold){
                        value.Input.EnableButtons((button, true));
                    }
                }
            }
            _interactingPlayer = value;
        }
    }

    /**
     * Every interactive object can be assigned a specific color so that only players with the right color can
     * interact with it.
     */
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    public PlayerColor Color => color;

    /**
     * Whether a player is triggering an interaction. See also class description.
     */
    private bool _isInteracting = false;

    public bool IsInteracting
    {
        get => _isInteracting && !ReferenceEquals(InteractingPlayer, null);
        private set
        {
            // If the value changed and a player is currently interacting with this object, fire the `interactionTriggeredEvent`
            if (value != _isInteracting && value)
            {
                interactionTriggeredEvent?.Invoke(InteractingPlayer);
                OnInteractionTriggered?.Invoke(InteractingPlayer);
            }
            
            _isInteracting = value;
        }
    }

    /**
     * While true, no interaction hint speech bubbles will be displayed
     */
    [NonSerialized]
    public bool SuppressSpeechBubble = false;

    private Collider2D _collider;
    
    /**
     * <summary>
     * Event which is fired, when a player starts interacting with this object
     *
     * DEPRECATED: Use <see cref="OnInteractionTriggered"/> instead.
     * </summary>
     */
    [Obsolete("Use OnInteractionTriggered instead.")]
    [SerializeField] private InteractionEvent interactionTriggeredEvent = default;

    /**
     * Event which is fired, when a player starts interacting with this object
     */
    public event InteractionTriggeredAction OnInteractionTriggered;
    public delegate void InteractionTriggeredAction(PlayerController player);

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }
    
    private void Update()
    {
        // If a player is currently touching this object...
        if (!ReferenceEquals(InteractingPlayer, null))
        {
            // Maybe the player can no longer interact, then we have to abort all interaction
            if (!InteractingPlayer.InteractorCollider.CanInteract)
            {
                ClearInteractingPlayer();
                return;
            }
            
            // Determine whether the player is pressing the interaction button in the right way:
            switch (interactionType)
            {
                case InteractionType.Down:
                    IsInteracting = InteractingPlayer.Input.GetButtonDown(button);
                    break;
                case InteractionType.Up:
                    IsInteracting = InteractingPlayer.Input.GetButtonUp(button);
                    break;
                case InteractionType.Hold:
                    IsInteracting = InteractingPlayer.Input.GetButton(button);
                    break;
            }
            
            if (IsInteracting)
            {
                InteractingPlayer.InteractionSpeechBubble.HideBubble();
            }
        }
    }

    private bool IsPlayerFacingThisObject(PlayerController player)
    {
        return player.IsLookingAt(_collider.bounds);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // only display speechbubbles and buttons when the interactive is enabled
        if (enabled)
        {
            // If there is not already a player touching this object,
            // if the other object touching this object is a player
            // and if this player has a color compatible with the one of this object,
            // and if this player is looking at this object,
            // remember the object as the player who is potentially interacting with this object.
            //
            // See also the `Update` method
            if (!IgnoreCollisions && ReferenceEquals(InteractingPlayer, null))
            {
                // Players have a special collider child object `InteractorCollider` for
                // and we use this one to detect collisions with the player.
                //
                // See the class description of `InteractorCollider` for more information
                if (other.gameObject.CompareTag("InteractorCollider"))
                {
                    var interactorCollider = other.gameObject.GetComponent<InteractorCollider>();
                    var maybeInteractingPlayer = interactorCollider.Player;
                    if (
                        interactorCollider.CanInteract &&
                        maybeInteractingPlayer.gameObject != this.gameObject && // do not interact with self
                        Color.IsCompatibleWith(maybeInteractingPlayer.Color) &&
                        IsPlayerFacingThisObject(maybeInteractingPlayer)
                    )
                    {
                        InteractingPlayer = maybeInteractingPlayer;
                        
                        // Furthermore, we want to display a speech bubble on the player, which informs about the possible
                        // interactions with this object.
                        if (!SuppressSpeechBubble)
                        {
                            InteractingPlayer.InteractionSpeechBubble.DisplayBubble(button);
                        }
                    }
                }
            }
            // If there has been a player interacting with this object
            // and if this player stopped looking at this object,
            // the player also stops interacting:
            else if (!ReferenceEquals(InteractingPlayer, null) && !IsPlayerFacingThisObject(InteractingPlayer))
            {
                ClearInteractingPlayer();
            }
        }
    }

    /**
     * If a player is currently interacting with this object, the interaction is stopped by this method.
     */
    private void ClearInteractingPlayer()
    {
        // ...no longer display a speech bubble about the interaction
        InteractingPlayer?.InteractionSpeechBubble.HideBubble();
        // ...no longer remember that player
        InteractingPlayer = null;
        // ...and interrupt any interaction that might have taken place
        IsInteracting = false;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // If there has been a player touching this object and the object exiting the collision is this player...
        if (
            !IgnoreCollisions &&
            !ReferenceEquals(InteractingPlayer, null) &&
            other.CompareTag("InteractorCollider") &&
            InteractingPlayer.gameObject == other.GetComponent<InteractorCollider>().Player.gameObject
        )
        {
            ClearInteractingPlayer();
        }
    }

    private void OnDisable()
    {
        // When this object is destroyed, sometimes OnExitTrigger2D is not executed (for example when changing the scene),
        // IN that case, we still need to clear up the interaction
        // (otherwise the player will keep the interaction controller button in the next scene etc.)
        ClearInteractingPlayer();
    }

    /**
     * Only called in editor, e.g. when changing a property
     */
    private void OnValidate()
    {
        // `Interactive` can be used in conjunction with the `Box` behavior which must change sprites in the editor
        // depending on the color value of this `Interactive` object.
        // 
        // Hence, we inform `Box` about property changes in this behavior.
        if (GetComponent<Box>() is Box box)
        {
            box.OnValidate();
        }
    }
}

[Serializable]
class InteractionEvent : UnityEvent<PlayerController>
{ }
