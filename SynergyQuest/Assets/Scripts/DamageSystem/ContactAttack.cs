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
using DamageSystem;
using UnityEngine;

/**
 * <summary>
 * Attacks any <see cref="Attackable"/> that collides with this object (trigger and true collision).
 * </summary>
 */
public class ContactAttack : MonoBehaviour
{
    [SerializeField, Tooltip("Negative values heal!")] private int damage = 1;
    [SerializeField, Tooltip("Units by which target will be knocked back.")] private float knockback = 4;

    [SerializeField, Tooltip("Frequency of attacks (in Hz), if target stays in contact with this object. No repeated attack if set to 0.")] private float frequency = 1;
    [SerializeField, Tooltip("If this is set, the set object will be identified as attacker instead of this.gameObject")] private GameObject customAttacker = default;
    [SerializeField, Tooltip("Targets with these tags will not receive attacks when coming in contact")]
    private string[] ignoredTargetTags = new string[0];

    [SerializeField, Tooltip("Targets with these tags will be knocked back while receiving no damage")]
    private string[] knockbackNoDamageTargetTags = new string[0];
    /**
     * <summary>
     * In the inspector, we can only set <see cref="ignoredTargetTags"/> as an array.
     * Thus, at runtime, we compute an optimized hashset from the array and store it here.
     * </summary>
     */
    private HashSet<string> _optimizedIgnoredTargetTags;

    /**
     * <summary>
     * In the inspector, we can only set <see cref="knockbackNoDamageTargetTags"/> as an array.
     * Thus, at runtime, we compute an optimized hashset from the array and store it here.
     * </summary>
     */
    private HashSet<string> _optimizedKnockbackTargetTags;
    /**
     * <summary>
     * Game objects subject to attack that currently in contact with this object (keys).
     * The stored value per object is a tuple of...
     * ...(1) the objects Attackable component
     * ...(2) the time of the last attack on this target.
     * 
     * It is used to continuously apply attacks with <see cref="frequency"/>.
     * </summary>
     */
    private Dictionary<GameObject, (Attackable, float)> _currentTargets = new Dictionary<GameObject, (Attackable, float)>();

    private void Awake()
    {
        // compute an optimized version of the collection of tags of objects which shall knocked back or not be attacked 
        _optimizedIgnoredTargetTags = new HashSet<string>(ignoredTargetTags);
        _optimizedKnockbackTargetTags = new HashSet<string>(knockbackNoDamageTargetTags);
        
        // if no custom attacker is set in the inspector, set the field to a true null value
        // (otherwise it seems we get some weird Unity thing that only compares by == to null)
        if (customAttacker == null)
        {
            customAttacker = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        OnTriggerEnter2D(other.collider);
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        OnTriggerStay2D(other.collider);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        OnTriggerExit2D(other.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if the colliding object is an Attackable and its tag is not ignored or it is able to be knocked back ...
        if (other.TryGetComponent(out Attackable attackable) && (!_optimizedIgnoredTargetTags.Contains(other.tag) 
                                                                 || _optimizedKnockbackTargetTags.Contains(other.tag)))
        {
            //... then we attack it
            PerformAttack(attackable, other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // If the object staying in contact is being attacked by us...
        if (_currentTargets.TryGetValue(other.gameObject, out (Attackable attackable, float lastAttackTime) otherData))
        {
            // ...and if its time to attack it again according to our attack frequency...
            if (Time.time - otherData.lastAttackTime > 1 / frequency)
            {
                // ...then we perform another attack on it.
                PerformAttack(otherData.attackable, other);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _currentTargets.Remove(other.gameObject);
    }

    /**
     * <summary>
     * Attacks an <see cref="Attackable"/> and remembers the time of the attack, so that it can be continously attacked
     * according to the attack frequency using <see cref="OnCollisionStay2D"/> / <see cref="OnTriggerStay2D"/>.
     * </summary>
     */
    private void PerformAttack(Attackable target, Collider2D targetCollider)
    {
        // ReSharper disable once Unity.NoNullCoalescing
        var attacker = customAttacker ?? this.gameObject;
        var attackDirection = (Vector2) (targetCollider.transform.position - attacker.transform.position).normalized;
        
        // Make sure the attacker does not attack itself
        if (!ReferenceEquals(target.gameObject ,attacker))
        {
            
            // Decide between knockback or damage attack
            if (_optimizedKnockbackTargetTags.Contains(targetCollider.tag))
            {
                target.Attack(
                    new WritableAttackData
                    {
                        Damage = 0,
                        Knockback = 7,
                        Attacker = attacker,
                        AttackDirection = Optional<Vector2>.Some(attackDirection)
                    }
                );
            }
            else
            {
                target.Attack(
                    new WritableAttackData
                    {
                        Damage = damage,
                        Knockback = knockback,
                        Attacker = attacker,
                        AttackDirection = Optional<Vector2>.Some(attackDirection)
                    }
                );
            }
            
        }
        _currentTargets[target.gameObject] = (target, Time.time);
    }
}
