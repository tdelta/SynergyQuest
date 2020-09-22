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
    
    private void OnEnable()
    {
        if (TryGetComponent(out Colored colored))
        {
            colored.OnColorChanged += OnPlayerColorChanged;
        }
    }
    
    private void OnDisable()
    {
        if (TryGetComponent(out Colored colored))
        {
            colored.OnColorChanged -= OnPlayerColorChanged;
        }
    }

    private void OnValidate()
    {
        if (TryGetComponent(out Colored colored))
        {
            OnPlayerColorChanged(colored.Color);
        }
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

    private void OnPlayerColorChanged(PlayerColor playerColor)
    {
        Color = playerColor;
    }
}
