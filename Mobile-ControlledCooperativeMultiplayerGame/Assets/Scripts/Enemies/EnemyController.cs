using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour {
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

    protected virtual void OnCollisionEnter2D(Collision2D other) {
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player == null) {
            direction = -direction;
            return;
        } 
        
        /*
        To be disscussed (Marc): Von wo kommt der Damage. putDamage durch den 
        Player oder hier.


        else if (isInvincible)
                return;

        invincibleTimer = timeInvincible;
        isInvincible = true;
        animator.SetTrigger("Hit");
        healthPoints -= player.getDamage();

        if (healthPoints == 0) {
            isDead = true;
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1);
        }
        */

    }

    public void putDamage(int amount){
        if(isInvincible) {
            return;
        }
        invincibleTimer = timeInvincible;
        isInvincible = true;
        animator.SetTrigger("Hit");
        healthPoints -= amount;

        if (healthPoints == 0) {
            isDead = true;
            animator.SetTrigger("Dead");
            Destroy(gameObject, 1);
        }
    }

    protected abstract Vector2 computeNewOffset();
    
    protected virtual void FixedUpdate() {
        if (!isDead) {
            Vector2 position = rigidbody2D.position;
            position += computeNewOffset();
            rigidbody2D.MovePosition(position);
        }
    }

    public int getDamage() {
        return damageFactor;
    }
}
