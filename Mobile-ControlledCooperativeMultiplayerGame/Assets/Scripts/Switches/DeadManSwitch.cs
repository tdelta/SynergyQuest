using UnityEngine;

/**
 * A switch must be constantly pressed to be activated.
 * It is build on top the base `Switch` component
 *
 * This behavior must be used in conjunction with the following behaviors:
 *
 * - SpriteRenderer
 * - Interactable
 * - Switch
 * - AudioSource
 */
public class DeadManSwitch : MonoBehaviour
{
    /**
     * Sprites to show when the switch is either on or off
     */
    [SerializeField] private Sprite switchOnSprite;
    [SerializeField] private Sprite switchOffSprite;
    /**
     * Sound to play when switch is used
     */
    [SerializeField] private AudioClip switchSound;

    private SpriteRenderer _renderer;
    private Interactive _interactive;
    private Switch _switch;
    private AudioSource _audioSource;

    /**
     * Caches, whether the switch was pressed in the previous state
     */
    private bool _previousInteractibleState = false;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _interactive = GetComponent<Interactive>();
        _switch = GetComponent<Switch>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Determine whether a player is using the switch
        var isInteracting = _interactive.IsInteracting;

        // If this changed since the last frame, we must update a few properties
        if (_previousInteractibleState != isInteracting)
        {
            // Play a sound indicating that the switch has been used
            _audioSource.PlayOneShot(switchSound);
            
            // Inform the base switch component, that the value of the switch changed
            _switch.Value = _interactive.IsInteracting;

            // Change the sprite accordingly
            _renderer.sprite = isInteracting ? switchOnSprite : switchOffSprite;
            
            _previousInteractibleState = isInteracting;
        }
    }
}
