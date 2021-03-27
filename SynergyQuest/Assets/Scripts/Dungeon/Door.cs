// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

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
 * Also it provides a method to load this room: <see cref="UseDoor"/>. Add the <see cref="ContactSwitch"/> behavior
 * and a <see cref="Switchable"/> to a (sub-)object with the door collider and assign it to
 * <see cref="contactSwitchable"/> if you want to load the room when a player makes contact with the door.
 *
 * Furthermore, a transition type can be defined, which determines the transition animation to use when loading the next
 * room. See also the <see cref="TransitionController"/> and <see cref="SceneController"/> singletons.
 *
 * A door can be open or closed. For this, objects using this behavior must also assign an (sub-)object with a
 * <see cref="Switchable"/> component to <see cref="lockSwitchable"/>.
 * The door is closed, if there is a connected switch, which is not triggered.
 * If the door is closed, it does not transition to the next room and its sprite changes accordingly.
 *
 * If a player is currently undergoing the <see cref="ReviveMinigame"/> after dying, a door can also not be used to
 * transition to the next scene, until all players are alive again. Instead, a speech bubble with a hint is displayed
 * using <see cref="SpeechBubble"/>.
 * </remarks>
 */
[RequireComponent(typeof(SpriteRenderer), typeof(AudioSource))]
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
    
    [SerializeField] private Switchable lockSwitchable;
    [SerializeField] private Switchable contactSwitchable;
    
    /**
     * Stores whether the door is open or closed
     */
    private bool _open = true;

    public string DoorId => doorId;
    public TransitionType TransitionType => transitionType;

    public void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        lockSwitchable.OnActivationChanged += OnLockActivationChanged;
        contactSwitchable.OnActivationChanged += OnContactActivationChanged;
    }
    
    private void OnDisable()
    {
        lockSwitchable.OnActivationChanged -= OnLockActivationChanged;
        contactSwitchable.OnActivationChanged -= OnContactActivationChanged;
    }

    private void Start()
    {
        // When starting, refresh the opened/closed state and sprites, depending on the state of all connected switches
        _open = lockSwitchable.Activation;
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
    public void UseDoor()
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
                    .Select(player => $"<color=#{ColorUtility.ToHtmlStringRGB(player.Color.ToRGB())}>{player.Data.Name}</color>")
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
     * Called, if the state of the assigned <see cref="lockSwitchable"/> component changes
     */
    private void OnLockActivationChanged(bool activation)
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
     * Called, if the state of the assigned <see cref="contactSwitchable"/> component changes
     */
    private void OnContactActivationChanged(bool activation)
    {
        if (activation)
        {
            UseDoor();
        }
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
