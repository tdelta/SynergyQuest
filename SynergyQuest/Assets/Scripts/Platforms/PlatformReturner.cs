using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Behavior for a pressure plate which resets a <see cref="Platform"/> to its original position when starting the scene.
 * If players are standing on the platform, their positions are reset, too, relative to the platform.
 * It deactivates itself, if the platform did not move (too much) from its original position.
 *
 * It uses the <see cref="Teleport"/> effect when changing the position of players and platforms for better visuals.
 * </summary>
 */
[RequireComponent(typeof(Switchable), typeof(ColorReplacer), typeof(ReplacementColorBrightener))]
public class PlatformReturner : MonoBehaviour
{
    [Tooltip("Platform game object which shall be moved by this behavior.")]
    [SerializeField] private GameObject platformToReturn = default;
    
    [Tooltip("Some platform implementations have their collider to detect collisions with players on a child object. Hence, the collider of the platform must be provided here manually.")]
    [SerializeField] private Collider2D platformPlayerCollider = default;
    
    [Tooltip("The platform must be at least this far from its original position so that the returner triggers a teleport back.")]
    [SerializeField] private float minimumDistanceFromReturnPoint = 0.1f;
    
    [Tooltip("The ColorReplacer component is used to color the sprite of this pressure plate. This color is used, when the platform is not far enough away to be returned.")]
    [SerializeField] private Color disabledColor = new Color(0.4f, 0.4f, 0.4f);
    [Tooltip("The ColorReplacer component is used to color the sprite of this pressure plate. This color is used, when the platform is far enough away to be returned.")]
    [SerializeField] private Color enabledColor = new Color(0.8f, 0.8f, 0.8f);

    /**
     * Position to which the platform is returned when this behavior activates.
     */
    private Vector3 _platformReturnPoint;
    
    private Switchable _switchable;
    private ColorReplacer _colorReplacer;

    private void Awake()
    {
        _switchable = GetComponent<Switchable>();
        _colorReplacer = GetComponent<ColorReplacer>();

        // Start in disabled state, color sprite accordingly
        _colorReplacer.ReplacementColor = disabledColor;
        // Remember the current position of the platform in the Awake phase as position to return to
        _platformReturnPoint = platformToReturn.transform.position;
    }
    
    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _colorReplacer.ReplacementColor = disabledColor;
        
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Start()
    {
        // We do not need to check every frame, whether the platform left its return point.
        // Instead, we do it every 0.5s which is enough
        InvokeRepeating(nameof(CheckIfPlatformLeft), 0.0f, 0.5f);
    }

    /**
     * <summary>
     * Checks whether the platform left its return point. If so, this method will enable the <see cref="Switchable"/>
     * which can trigger the return of the platform.
     *
     * Futhermore, we adjust the coloring of the sprite.
     * </summary>
     */
    void CheckIfPlatformLeft()
    {
        // Only do something, if this component is currently enabled.
        // We check this, since this method is invoked using `InvokeRepeating`, which is not paused when this component
        // is disabled
        if (this.enabled)
        {
            // Determine, whether the platform has left its return point.
            // That is, when its distance to the return point exceeds the set threshold.
            var hasPlatformLeft = Vector3.Distance(_platformReturnPoint, platformToReturn.transform.position)
                                  > minimumDistanceFromReturnPoint;

            _switchable.enabled = hasPlatformLeft;
            _colorReplacer.ReplacementColor = hasPlatformLeft ? enabledColor : disabledColor;
        }
    }

    /**
     * <summary>
     * Invoked when the <see cref="Switchable"/> component of this object changes its activation.
     * Teleports the platform and the players on it back to the return point when the <see cref="Switchable"/> is
     * activated.
     * </summary>
     */
    private void OnActivationChanged(bool activation)
    {
        if (activation)
        {
            ReturnPlatform();
        }
    }

    /**
     * <summary>
     * Teleports the platform and the players on it back to the return point by setting their transform positions.
     * Also uses the <see cref="Teleport"/> behavior for visual effect.
     * </summary>
     */
    private void ReturnPlatform()
    {
        // == 1. Find all objects of the "Player" layer which are in contact with the platform ==
        var playerContactFilter = new ContactFilter2D();
        playerContactFilter.SetLayerMask(LayerMask.GetMask("Player"));

        var contactsOfPlatform = new List<Collider2D>();
        platformPlayerCollider
            .OverlapCollider(playerContactFilter, contactsOfPlatform);

        // == 2. Of those objects, only keep the players and determine their position relative to the platform ==
        var platformPosition = platformToReturn.transform.position;
        var playersInContact = contactsOfPlatform
            // Get the players of the objects in contact
            .SelectNotNull(collider => collider.GetComponent<PlayerController>())
            // For each player, build a pair containing the player and its position relative to the platform
            .Select(player =>
                (
                    player,
                    player.transform.position - platformPosition
                )
            )
            .ToArray();

        // == 3. Teleport out all players standing on the platform (visual effect) ==
        foreach (var (player, _) in playersInContact)
        {
            Teleport.TeleportOut(player.gameObject, player.Color.ToRGB(), null, 0.7f);
        }
        
        // == 4. Teleport out the platform (visual effect, slightly delayed to ensure players teleport out first) ==
        Teleport.TeleportOut(
            platformToReturn,
            Color.white,
            () =>
            {
                // == 5. Move the platform to the return point, when the teleport-out effect finishes ==
                platformToReturn.transform.position = _platformReturnPoint;
                
                // == 6. Teleport in the platform again (visual effect)
                Teleport.TeleportIn(
                    platformToReturn,
                    Color.white,
                    null,
                    0.7f
                );

                foreach (var (player, relativePositionToPlatform) in playersInContact)
                {
                    // == 7. Move all players to the return point, offset by their relative position to the platform we determined beforehand ==
                    player.PhysicsEffects.Teleport(_platformReturnPoint + relativePositionToPlatform);
                    
                    // == 8. Teleport the players in (visual effect, slightly delayed, to ensure the platform appears first) ==
                    Teleport.TeleportIn(
                        player.gameObject,
                        player.Color.ToRGB(),
                        null,
                        0.75f
                    );
                }
            },
            0.75f
        );
    }
}
