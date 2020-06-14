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


    protected virtual void Start() {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();

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

        ChangeHealth(-amount);
    }

    /*
     * Each entity (player, enemy) should implement this method to control how it
     * is affected by health changes.
     */
    protected abstract void ChangeHealth(int amount);
}
