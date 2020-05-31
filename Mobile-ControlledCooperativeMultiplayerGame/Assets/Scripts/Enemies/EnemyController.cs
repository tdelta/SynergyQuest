using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour {
    public float timeInvincible = 1;
    public int   healthPoints = 1;

    protected new Rigidbody2D rigidbody2D;
    protected Animator animator;
    protected bool isDead;

    bool isInvincible;
    float invincibleTimer;

    protected virtual void Start() {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Update() {
        if (isInvincible) {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0) {
                isInvincible = false;
                animator.SetTrigger("Vulnerable");
            }
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D other) {
        if (isInvincible)
                return;

        invincibleTimer = timeInvincible;
        isInvincible = true;
        animator.SetTrigger("Hit");
        healthPoints--;

        if (healthPoints == 0) {
            isDead = true;
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1);
        }

    }

    protected abstract void FixedUpdate();
}
