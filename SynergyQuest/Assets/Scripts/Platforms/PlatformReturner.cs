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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Behavior for a pressure plate which resets a <see cref="Platform"/> to its original position when starting the scene.
 * If players are standing on the platform, their positions are reset, too, relative to the platform.
 * It deactivates itself, if the platform did not move (too much) from its original position
 * (its deactivated when the platform touches the trigger collider of this object).
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
    
    [Tooltip("The ColorReplacer component is used to color the sprite of this pressure plate. This color is used, when the platform is not far enough away to be returned.")]
    [SerializeField] private Color disabledColor = new Color(0.4f, 0.4f, 0.4f);
    [Tooltip("The ColorReplacer component is used to color the sprite of this pressure plate. This color is used, when the platform is far enough away to be returned.")]
    [SerializeField] private Color enabledColor = new Color(0.8f, 0.8f, 0.8f);

    /**
     * Position to which the platform is returned when this behavior activates.
     */
    private Vector3 _platformReturnPoint;
    private bool _didPlatformLeave = true;
    
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ReferenceEquals(other.gameObject, platformToReturn))
        {
            OnPlatformLeaveOrReturn(false);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (ReferenceEquals(other.gameObject, platformToReturn))
        {
            OnPlatformLeaveOrReturn(true);
        }
    }

    /**
     * <summary>
     * If the platform left its starting region (that is, the region covered by the trigger collider of this object),
     * this method will enable the <see cref="Switchable"/>
     * which can trigger the return of the platform.
     *
     * Futhermore, we adjust the coloring of the sprite.
     * </summary>
     */
    void OnPlatformLeaveOrReturn(bool didPlatformLeave)
    {
        _switchable.enabled = didPlatformLeave;
        _colorReplacer.ReplacementColor = didPlatformLeave ? enabledColor : disabledColor;
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
