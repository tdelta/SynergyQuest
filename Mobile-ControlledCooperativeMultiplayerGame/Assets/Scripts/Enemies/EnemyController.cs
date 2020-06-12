using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : EntityController {
    public int   healthPoints = 1;
    public float directionSpeed = 1;
    public float directionChangeTime = 1;
    public int damageFactor = 1;
    public ParticleSystem smokeEffect;

    protected float directionTimer;
    protected Vector2 direction;
    
    bool isDead;
    readonly int deadTrigger = Animator.StringToHash("Dead");


    protected virtual void Start() {
        base.Start();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
    }

    protected override void Update() {
        base.Update();
        directionTimer -= Time.deltaTime;

        if (directionTimer < 0) {
            direction = Random.insideUnitCircle.normalized;
            directionTimer = directionChangeTime;
        }
    }

    void OnCollisionStay2D(Collision2D other) {
        if (other.gameObject.tag == "Player") {
            var player = other.gameObject.GetComponent<EntityController>();
            player.PutDamage(damageFactor, (other.transform.position - transform.position).normalized); 
        } else
            direction = -direction;
    }

    protected override void ChangeHealth(int amount) {
        healthPoints += amount;

        if (healthPoints <= 0) {
            isDead = true;
            animator.SetTrigger(deadTrigger);
            Destroy(gameObject, 1);
        }
    }

    /*
     * Different enemy types only differ in their movements. New enemies should implement this method,
     * to define their behaviour.
     */
    protected abstract Vector2 ComputeOffset();
    
    void FixedUpdate() {
        if (!isDead) {
            Vector2 position = rigidbody2D.position;
            position += ComputeOffset();
            rigidbody2D.MovePosition(position);
        }
    }

    public void ShowParticles() {
        smokeEffect.Play();
    }
}
