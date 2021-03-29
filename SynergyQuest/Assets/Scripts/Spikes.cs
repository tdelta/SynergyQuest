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

using Audio;
using DamageSystem;
using UnityEngine;
using Utils;

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
[RequireComponent(typeof(SharedAudioSource))]
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
    private SharedAudioSource _sharedAudioSource;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _switchable = GetComponent<Switchable>();
        _collider = GetComponent<BoxCollider2D>();
        _sharedAudioSource = GetComponent<SharedAudioSource>();
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
        ChangeSpikeState(_switchable.Activation, false);
    }

    /**
     * <summary>Deal damage to <see cref="Attackable"/> when in contact.</summary>
     * <remarks>
     * FIXME: Use <see cref="ContactAttack"/> component instead of this.
     * </remarks>
     */
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out Attackable target))
        {
            target.Attack(new WritableAttackData
            {
                Attacker = gameObject,
                Damage = damage,
                AttackDirection = Optional.Some(Vector2.zero)
            });
        }
    }

    /**
     * This callback is invoked, when all required switches to enable/disable the spikes change their state,
     * see also the `Switchable` component.
     */
    void OnActivationChanged(bool activation)
    {
        ChangeSpikeState(activation, true);
    }

    void ChangeSpikeState(bool activation, bool playSound)
    {
        // The spikes are enabled, when the switch is not active
        // (Currently we only use this component together with DeadManSwitches)
        var spikesAreOn = !activation;
        
        if (spikesAreOn)
        {
            if (playSound)
            {
                _sharedAudioSource.GetAudioSource().PlayOneShotIfAvailable(spikesUpSound);
            }
            _renderer.sprite = spikesOnSprite;
            _collider.enabled = true;
        }
        
        else
        {
            if (playSound)
            {
                _sharedAudioSource.GetAudioSource().PlayOneShotIfAvailable(spikesDownSound);
            }
            _renderer.sprite = spikesOffSprite;
            _collider.enabled = false;
        }
    }
}
