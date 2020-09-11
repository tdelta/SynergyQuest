using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Controls doors between dungeon rooms.
 * This behavior should be placed on every door of a dungeon room.
 * </summary>
 * <remarks>
 * This behaviour records the ID of the door, which is used by the <see cref="DungeonLayout"/> class to determine which
 * room lies behind the door.
 * Also it provides a method to load this room. This behavior may be used in conjunction with the
 * <see cref="ContactTrigger"/> behavior to trigger this method when a player touches the door.
 *
 * Furthermore, a transition type can be defined, which determines the transition animation to use when loading the next
 * room. See also the <see cref="TransitionController"/> and <see cref="SceneController"/> singletons.
 *
 * A door can be open or closed. Objects using this behavior must also add a <see cref="Switchable"/> component to it.
 * The door is closed, if there is a connected switch, which is not triggered.
 * If the door is closed, it does not transition to the next room and its sprite changes accordingly.
 *
 * If a player is currently undergoing the <see cref="ReviveMinigame"/> after dying, a door can also not be used to
 * transition to the next scene, until all players are alive again. Instead, a speech bubble with a hint is displayed
 * using <see cref="SpeechBubble"/>.
 * </remarks>
 */
[RequireComponent(typeof(Switchable), typeof(SpriteRenderer), typeof(AudioSource))]
public class Door : MonoBehaviour
{
    /**
     * Identifier of this door. Should be the same as the one set in the layout file of the dungeon.
     */
    [SerializeField] private string doorId = default;
    /**
     * Which transition animation to play when using the door to switch scenes
     */
    [SerializeField] private TransitionType transitionType = TransitionType.None;

    /**
     * Sprite to display if the door is open
     */
    [SerializeField] private Sprite openSprite = default;
    /**
     * Sprite to display if the door is closed
     */
    [SerializeField] private Sprite closedSprite = default;

    /**
     * The direction to which this door leads. This direction will be used to determine how the players should be
     * oriented after entering the next room.
     */
    [SerializeField] private Direction direction = default;

    private SpriteRenderer _renderer;
    private AudioSource _audioSource;
    private Switchable _switchable;
    /**
     * Stores whether the door is open or closed
     */
    private bool _open = true;

    public string DoorId => doorId;
    public TransitionType TransitionType => transitionType;

    public void Awake()
    {
        _switchable = GetComponent<Switchable>();
        _renderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Start()
    {
        // When starting, refresh the opened/closed state and sprites, depending on the state of all connected switches
        _open = _switchable.Activation;
        UpdateSprite();
    }

    /**
     * Use this door to load the next room.
     * A dungeon layout must be loaded for this to work, see also the `DungeonLayout` singleton.
     *
     * You can use an instance of the `DungeonLoader` behavior to ensure that the dungeon layout is loaded for a room.
     *
     * This method has no effect, if the door is still locked (with a key lock or due to some other mechanism)
     */
    public void UseDoor(PlayerController user)
    {
        if (_open)
        {
            var playersInRevivalMinigame = ReviveMinigame.GetPlayersInMinigame();

            // Did some players die and are currently part of the ReviveMinigame?
            // In this case, display a hint and dont switch scenes
            if (playersInRevivalMinigame.Any())
            {
                // Get the name of all players in the minigame (and color their name in their player color)
                var playerNames = playersInRevivalMinigame
                    .Select(player => $"<color=#{ColorUtility.ToHtmlStringRGB(player.Color.ToRGB())}>{player.Data.name}</color>")
                    .ToArray();
                
                var init = string.Join(", ", playerNames.Init());
                var playerListString = init.Any() ? $"{init} and {playerNames.Last()}" : playerNames.Last();

                // Display a speech bubble with a hint: All players must be alive to enter the next scene
                SpeechBubble.Display(this.transform.position, $"Revive {playerListString} before leaving!", 3.0f);
            }

            // Otherwise we can switch scenes
            else
            {
                PlayerDataKeeper.Instance.LastDoorDirection = this.direction;
                DungeonLayout.Instance.LoadRoomUsingDoor(this);
            }
        }
    }

    /**
     * Called, if the state of the connected `Switchable` component changes
     */
    private void OnActivationChanged(bool activation)
    {
        // the door is opened, if connected switches are active
        if (_open != activation && activation)
        {
            _audioSource.Play();
        }
        _open = activation;
        
        UpdateSprite();
    }

    /**
     * Changes the sprite depending on whether the door is open or closed
     */
    private void UpdateSprite()
    {
        var sprite = _open ? openSprite : closedSprite;
        _renderer.sprite = sprite;
    }
}
