using DamageSystem;
using UnityEngine;

public class Attackable : MonoBehaviour
{
    [SerializeField, Tooltip("How long the entity is immune to attacks after a successful hit")] private float invincibleTime = 0.8f;
    public float InvincibleTime => invincibleTime;
    public bool IsInvincible { get; private set; } = false;

    public event InvincibilityChangedAction OnInvincibilityChanged;
    public delegate void InvincibilityChangedAction(bool isInvincible);
    
    public event AttackAction OnPendingAttack;
    public event AttackAction OnAttack;
    public delegate void AttackAction(AttackData attack);
    
    public void Attack(AttackData attack)
    {
        if (this.enabled)
        {
            if (IsInvincible)
            {
                return;
            }
            
            OnPendingAttack?.Invoke(attack);
            OnAttack?.Invoke(attack);

            if (attack.damage > 0)
            {
                IsInvincible = true;
                OnInvincibilityChanged?.Invoke(IsInvincible);
                StartCoroutine(
                    CoroutineUtils.Wait(invincibleTime, () =>
                    {
                        IsInvincible = false;
                        OnInvincibilityChanged?.Invoke(IsInvincible);
                    })
                );
            }
        }
    }
}
