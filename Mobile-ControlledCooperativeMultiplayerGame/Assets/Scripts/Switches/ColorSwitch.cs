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

    void Awake()
    {
        _switch = GetComponent<Switch>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (
            _activatingPlayer is null &&
            other.CompareTag("Player") &&
            other.GetComponent<PlayerController>() is PlayerController player &&
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
