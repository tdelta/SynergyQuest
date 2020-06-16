using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EntityController : MonoBehaviour {
    [SerializeField] float timeInvincible = 1;

    protected Animator animator;
    protected Rigidbody2D rigidbody2D;
    protected bool isInvincible;
    protected float invincibleTimer;

    readonly int vulnerableTrigger = Animator.StringToHash("Vulnerable");
    readonly int hitTrigger = Animator.StringToHash("Hit");

    protected float TimeInvincible => timeInvincible;

    protected PhysicsEffects effects;

    protected virtual void Start() {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();

        effects = new PhysicsEffects(rigidbody2D);
    }

    protected virtual void Update() {
        if (isInvincible) {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0) {
                isInvincible = false;
                animator.SetTrigger(vulnerableTrigger);
            }
        }
    }

    /**
     * Apply knockback on damage.
     * EXPERIMENTAL FUNCTION
     */
    private IEnumerator KnockbackCoroutine(Vector2 direction)
    {
        //Instead of starting with some fixed speed which is then decelerated, we maybe should use the
        //proper equations for impulses and the effects of friction etc.
        var speed = 5.0f;
        var acceleration = -9.0f;
        var duration = 1.0f;
        var timer = 0.0f;
        while (timer <= duration)
        {
            timer += Time.deltaTime;
            speed = Mathf.Max(0, speed + acceleration * Time.deltaTime);
            
            // Ok so modifying position directly is usually a big no-no, but hear me out:
            // 
            // * The proper way to apply knockback in Unity's physics engine would be AddForce, however, this would also
            //   require us to use AddForce for motion instead of MovePosition, since MovePosition cancels out AddForce
            // * Using AddForce for motion does not feel right for a Zelda-like game, since it leads to sliding effects
            //   etc. Hence we are using MovePosition
            // * Since we are using position manipulation to simulate the linear and abrupt movement we want, we also
            //   need to simulate the effect of forces ourselves
            // * This coroutine implements this simulation for knockback by manipulating the position
            // Remaining explanation TODO
            // 
            this.rigidbody2D.position =
                this.rigidbody2D.position + direction * 1 * (Time.deltaTime * speed);

            yield return null;
        }
    }

    /*
     * This method can be used to damage an entity (player, enemy) on collision.
     */
    public void PutDamage(int amount, Vector2 knockbackDirection) {
        if (isInvincible) {
            return;
        }
        animator.SetTrigger(hitTrigger);
        invincibleTimer = timeInvincible;
        isInvincible = true;
        
        // TODO: implement a working knockback mechanism
        //var stopForce = -rigidbody2D.velocity * rigidbody2D.mass;
        //rigidbody2D.AddForce(stopForce + amount * knockbackDirection, ForceMode2D.Impulse);
        //StartCoroutine(KnockbackCoroutine(knockbackDirection));
        effects.ApplyImpulse(knockbackDirection*4);

        ChangeHealth(-amount);
    }

    /*
     * Each entity (player, enemy) should implement this method to control how it
     * is affected by health changes.
     */
    protected abstract void ChangeHealth(int amount);
}
