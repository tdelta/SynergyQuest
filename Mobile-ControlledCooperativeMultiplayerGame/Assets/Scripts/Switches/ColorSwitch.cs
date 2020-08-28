using System;
using JetBrains.Annotations;
using UnityEngine;

/**
 * <summary>
 * Switch which reacts to players of certain color standing on it.
 * </summary>
 * <seealso cref="Switch"/>
 * <seealso cref="Switchable"/>
 */
[RequireComponent(typeof(Switch), typeof(ColorReplacer))]
public class ColorSwitch : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    public PlayerColor Color
    {
        get => color;
        set
        {
            color = value;
            ActivatingPlayer = null;
        }
    }

    private Switch _switch;

    [CanBeNull] private PlayerController _activatingPlayer = null;

    [CanBeNull]
    public PlayerController ActivatingPlayer
    {
        get
        {
            return _activatingPlayer;
        }

        private set
        {
            _activatingPlayer = value;

            if (!ReferenceEquals(_switch, null))
            {
                // Iff a activating player is set, the switch component shall be activated
                _switch.Value = !(value is null);
            }
        }
    }

    /**
     * <summary>
     * Set this property to the game object of some player, if this switch shall ignore contact with the player once.
     * This is useful for example when teleporting a player on top of this switch but the switch shall not be triggered
     * by the teleport.
     *
     * The property is reset to <c>null</c> the next time the player triggers the <see cref="OnTriggerEnter2D"/> event.
     * </summary>
     */
    [NonSerialized]
    public GameObject IgnoreContactOnce = null;

    void Awake()
    {
        _switch = GetComponent<Switch>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // See description of IgnoreContactOnce
        if (IgnoreContactOnce == other.gameObject)
        {
            IgnoreContactOnce = null;
            return;
        }
        
        if (
            _activatingPlayer is null &&
            other.CompareTag("Player") &&
            other.TryGetComponent<PlayerController>(out var player) &&
            player.Color.IsCompatibleWith(this.Color)
        )
        {
            ActivatingPlayer = player;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (
            !(_activatingPlayer is null) &&
            other.gameObject == _activatingPlayer.gameObject
        )
        {
            ActivatingPlayer = null;
        }
    }
}
