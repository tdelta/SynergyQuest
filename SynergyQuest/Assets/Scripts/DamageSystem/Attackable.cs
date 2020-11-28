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
using DamageSystem;
using UnityEngine;

/**
 * <summary>
 * An object with this behavior can be attacked.
 * That is, an <see cref="Attack"/> method is provided.
 * Furthermore, other behaviors can react to attacks by subscribing to the events <see cref="OnPendingAttack"/> or
 * <see cref="OnAttack"/>.
 * </summary>
 * <remarks>
 * As an example of a behavior utilizing these events, see <see cref="Health"/>.
 * </remarks>
 */
public class Attackable : MonoBehaviour
{
    [SerializeField, Tooltip("How long the entity is immune to attacks after a successful hit")]
    private float invincibleTime = 0.8f;
    // used internally to keep track on when this entity last became invincible, so that the invincibility can time out
    private float _invincibleTimer = -1;
    
    /**
     * <summary>
     * How long this entity is immune to attacks after a successful hit
     * </summary>
     */
    public float InvincibleTime => invincibleTime;

    public bool IsInvincible
    {
        get => _isInvincible;
        private set
        {
            if (_isInvincible != value)
            {
                _isInvincible = value;
                OnInvincibilityChanged?.Invoke(value);
            }
        }
    }
    private bool _isInvincible = false;

    /**
     * <summary>
     * Emitted when this object stops or starts being invincible.
     * </summary>
     * <seealso cref="InvincibleTime"/>
     */
    public event InvincibilityChangedAction OnInvincibilityChanged;
    public delegate void InvincibilityChangedAction(bool isInvincible);
    
    /**
     * <summary>
     * Emitted just before a new attack hits.
     * Modifying the <see cref="WritableAttackData"/> object is OK (and the purpose of this event).
     * </summary>
     * <remarks>
     * Other behaviors should subscribe this event if they want to modify attacks before they hit.
     * E.g. it can be used to cancel attacks.
     * As an example, the <see cref="NecromancerController"/> sets the damage of attacks to 0, if the attacking player
     * does not have a certain color.
     * </remarks>
     */
    public event PendingAttackAction OnPendingAttack;
    public delegate void PendingAttackAction(WritableAttackData attack);
    
    /**
     * <summary>
     * Emitted when a new attack hits.
     * </summary>
     */
    public event AttackAction OnAttack;
    public delegate void AttackAction(AttackData attack);

    private void Update()
    {
        if (IsInvincible && Time.time - _invincibleTimer > InvincibleTime)
        {
            IsInvincible = false;
        }
    }

    /**
     * <summary>
     * Attack this object.
     * </summary>
     * <remarks>
     * * has no effect if <see cref="IsInvincible"/> is true, or this behavior is not enabled
     * * the events <see cref="OnPendingAttack"/> and <see cref="OnAttack"/> will be triggered.
     * * if the damage is non-zero, this object will become invincible for <see cref="invincibleTime"/> seconds
     *   and the <see cref="OnInvincibilityChanged"/> event is emitted.
     * </remarks>
     */
    public void Attack(WritableAttackData attack)
    {
        if (IsInvincible)
        {
            return;
        }
        
        OnPendingAttack?.Invoke(attack);
        OnAttack?.Invoke(attack);

        if (attack.Damage > 0)
        {
            IsInvincible = true;
            _invincibleTimer = Time.time;
        }
    }
}
