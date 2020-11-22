using UnityEngine;

namespace DamageSystem
{
    public class AttackData
    {
        public GameObject attacker = null;
        public int damage = 0;
        public float knockback = 0;
        public Optional<Vector2> attackDirection = Optional<Vector2>.None();
    }
}