using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : EntityController {
    public float timeInvincible = 1;
    public int   healthPoints = 1;
    public float directionSpeed = 1;
    public float directionChangeTime = 1;
    public int damageFactor = 1;

    protected new Rigidbody2D rigidbody2D;
    protected Animator animator;
    protected float directionTimer;
    protected Vector2 direction;
    
    bool isDead;
    bool isInvincible;
    float invincibleTimer;

    protected virtual void Start() {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
    }

    protected virtual void Update() {
        directionTimer -= Time.deltaTime;

        if (directionTimer < 0) {
            direction = Random.insideUnitCircle.normalized;
            directionTimer = directionChangeTime;
        }

        if (isInvincible) {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0) {
                isInvincible = false;
                animator.SetTrigger("Vulnerable");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player") {
            var player = other.gameObject.GetComponent<EntityController>();
            player.putDamage(damageFactor, (other.transform.position - transform.position).normalized); 
        } else
            direction = -direction;
    }

    public override void putDamage(int amount, Vector2 knockbackDirection){
        if (isInvincible) {
            return;
        }
        invincibleTimer = timeInvincible;
        isInvincible = true;
        animator.SetTrigger("Hit");
        healthPoints -= amount;
        var stopForce = -rigidbody2D.velocity * rigidbody2D.mass;
        rigidbody2D.AddForce(stopForce + knockbackFactor * amount * knockbackDirection, ForceMode2D.Impulse);

        if (healthPoints == 0) {
            isDead = true;
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1);
        }
    }

    protected abstract Vector2 computeForce();
    
    void FixedUpdate() {
        if (!isDead) {
            rigidbody2D.AddForce(computeForce());
        }
    }
}
