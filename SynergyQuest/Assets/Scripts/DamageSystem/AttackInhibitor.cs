using UnityEngine;

namespace DamageSystem
{
    public interface AttackInhibitor
    {
        bool IsAttackSuccessful(GameObject attacker);
    }
}