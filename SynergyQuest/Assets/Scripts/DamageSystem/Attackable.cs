using System;
using System.Linq;
using DamageSystem;
using UnityEngine;

public class Attackable : MonoBehaviour
{
    public delegate void AttackedAction(GameObject attacker, Optional<Vector2> fromDirection);
    public event AttackedAction OnAttacked;

    private AttackInhibitor[] _attackInhibitors;

    private void Awake()
    {
        _attackInhibitors = BehaviourExtensions.GetComponentsByInterface<AttackInhibitor>(this);
    }

    public void Attack(GameObject attacker, Optional<Vector2> fromDirection)
    {
        if (_attackInhibitors.All(inhibitor => inhibitor.IsAttackSuccessful(attacker)))
        {
            OnAttacked?.Invoke(attacker, fromDirection);
        }
    }
}
