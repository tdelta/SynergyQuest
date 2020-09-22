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
    [SerializeField] private Sprite switchOnSprite = default;
    [SerializeField] private Sprite switchOffSprite = default;
    /**
     * Sound to play when switch is used
     */
    [SerializeField] private AudioClip switchSound = default;

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
