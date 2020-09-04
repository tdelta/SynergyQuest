using UnityEngine;

abstract public class EntityController : MonoBehaviour {
    [SerializeField] float timeInvincible = 1;

    private Animator _animator;
    protected Animator Animator => _animator;
    
    private Rigidbody2D _rigidbody2D;
    public Rigidbody2D Rigidbody2D => _rigidbody2D;
    
    protected bool isInvincible;
    protected float invincibleTimer;

    readonly int vulnerableTrigger = Animator.StringToHash("Vulnerable");
    readonly int hitTrigger = Animator.StringToHash("Hit");

    protected float TimeInvincible => timeInvincible;

    private PhysicsEffects _physicsEffects;
    public PhysicsEffects PhysicsEffects => _physicsEffects;


    protected virtual void Awake() {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
    }

    protected virtual void Start() {
    }

    protected virtual void Update() {
        if (isInvincible) {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0) {
                isInvincible = false;
                _animator.SetTrigger(vulnerableTrigger);
            }
        }
    }

    /*
     * This method can be used to damage an entity (player, enemy) on collision.
     */
    public void PutDamage(int amount, Vector2 knockbackDirection) {
        // if invincible or entitiy doesn't accept change in health do nothing
        if (isInvincible)
            return;
        

        if (ChangeHealth(-amount))
            PhysicsEffects.ApplyImpulse(knockbackDirection * 4);
        // if the entity doesn't accept change in health (e.g. flying player) only apply (reduced) knockback
        else {
            PhysicsEffects.ApplyImpulse(knockbackDirection * 2);
            return;
        }

        _animator.SetTrigger(hitTrigger);

        invincibleTimer = timeInvincible;
        isInvincible = true;
    }

    /*
     * Each entity (player, enemy) should implement this method to control how it
     * is affected by health changes.
     * The return value should indicate if the change in health actually happened
     */
    protected abstract bool ChangeHealth(int amount, bool playSounds = true);
}
