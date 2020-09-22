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
using UnityEngine;

/**
 * <summary>
 * Spikes are a zone which can hurt the player, but which can be enabled and disabled by switches.
 * It relies on the <see cref="Switchable"/> component to receive signals from switches, <see cref="Switch"/>, which can
 * enable or disable it.
 * </summary>
 * <seealso cref="Switch"/>
 * <seealso cref="Switchable"/>
 */
[RequireComponent(typeof(Switchable))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class Spikes : MonoBehaviour
{
    /**
     * The sprite which is shown when the spikes are active
     */
    [SerializeField] private Sprite spikesOnSprite = default;
    /**
     * The sprite which is shown when the spikes are retracted
     */
    [SerializeField] private Sprite spikesOffSprite = default;
    /**
     * Sound which is played when the spikes are activated
     */
    [SerializeField] private AudioClip spikesUpSound = default;
    /**
     * Sound which is played when the spikes are retracted
     */
    [SerializeField] private AudioClip spikesDownSound = default;
    /**
     * How much damage the spikes shall deal on contact
     */
    [SerializeField] private int damage = 2;
    
    private SpriteRenderer _renderer;
    private Switchable _switchable;
    private BoxCollider2D _collider;
    private AudioSource _audioSource;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _switchable = GetComponent<Switchable>();
        _collider = GetComponent<BoxCollider2D>();
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
        // The Switchable component does not trigger this callback by itself for the initial activation when loading the
        // scene. Hence, we look the initial value up ourselves
        OnActivationChanged(_switchable.Activation);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            var player = other.GetComponent<EntityController>();
            
            player.PutDamage(damage, Vector2.zero);
        }
    }

    /**
     * This callback is invoked, when all required switches to enable/disable the spikes change their state,
     * see also the `Switchable` component.
     */
    void OnActivationChanged(bool activation)
    {
        // The spikes are enabled, when the switch is not active
        // (Currently we only use this component together with DeadManSwitches)
        var spikesAreOn = !activation;
        
        if (spikesAreOn)
        {
            _audioSource.PlayOneShot(spikesUpSound);
            _renderer.sprite = spikesOnSprite;
            _collider.enabled = true;
        }
        
        else
        {
            _audioSource.PlayOneShot(spikesDownSound);
            _renderer.sprite = spikesOffSprite;
            _collider.enabled = false;
        }
    }
}
