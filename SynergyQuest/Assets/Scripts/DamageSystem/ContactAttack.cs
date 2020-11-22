using System;
using System.Collections.Generic;
using DamageSystem;
using UnityEngine;

public class ContactAttack : MonoBehaviour
{
    [SerializeField, Tooltip("Negative values heal!")] private int damage = 1;
    [SerializeField, Tooltip("Units by which target will be knocked back.")] private float knockback = 4;

    [SerializeField, Tooltip("Frequency of attacks (in Hz), if target stays in contact with this object. No repeated attack if set to 0.")] private float frequency = 1;
    [SerializeField, Tooltip("If this is set, the set object will be identified as attacker instead of this.gameObject")] private GameObject customAttacker = default;
    [SerializeField, Tooltip("Targets with these tags will not receive attacks when coming in contact")]
    private string[] ignoredTargetTags = new string[0];

    private HashSet<string> _optimizedIgnoredTargetTags;

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
        _optimizedIgnoredTargetTags = new HashSet<string>(ignoredTargetTags);
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
        if (other.TryGetComponent(out Attackable attackable) && !_optimizedIgnoredTargetTags.Contains(other.tag))
        {
            PerformAttack(attackable, other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_currentTargets.TryGetValue(other.gameObject, out (Attackable attackable, float lastAttackTime) otherData))
        {
            if (Time.time - otherData.lastAttackTime > 1 / frequency)
            {
                PerformAttack(otherData.attackable, other);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _currentTargets.Remove(other.gameObject);
    }

    private void PerformAttack(Attackable target, Collider2D targetCollider)
    {
        // ReSharper disable once Unity.NoNullCoalescing
        var attacker = customAttacker ?? this.gameObject;
        var attackDirection = (Vector2) (targetCollider.transform.position - attacker.transform.position).normalized;

        target.Attack(
            new AttackData
            {
                damage = damage,
                knockback = knockback,
                attacker = attacker,
                attackDirection = Optional<Vector2>.Some(attackDirection)
            }
        );

        _currentTargets[target.gameObject] = (target, Time.time);
    }
}
