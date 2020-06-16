using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : EntityController {
    [SerializeField] protected int   healthPoints = 1;
    [SerializeField] protected float directionSpeed = 1;
    [SerializeField] protected float directionChangeTime = 1;
    [SerializeField] protected int damageFactor = 1;
    [SerializeField] ParticleSystem smokeEffect;
    [SerializeField] private MultiSound hitSounds;

    protected float directionTimer;
    protected Vector2 direction;
    
    bool isDead;
    readonly int deadTrigger = Animator.StringToHash("Dead");

    /**
     * Used to briefly flash an enemy in a certain color. For example red when it is hit.
     */
    private TintFlashController _tintFlashController;

    protected override void Start() {
        base.Start();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
        
        _tintFlashController = GetComponent<TintFlashController>();
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
        if (other.gameObject.CompareTag("Player")) {
            var player = other.gameObject.GetComponent<EntityController>();
            player.PutDamage(damageFactor, (other.transform.position - transform.position).normalized); 
        } else
            direction = -direction;
    }

    protected override void ChangeHealth(int amount) {
        healthPoints += amount;

        if (amount <= 0)
        {
            PlayDamageEffects();
        }

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
            
            effects.MoveBody(position);
        }
    }

    public void ShowParticles() {
        smokeEffect.Play();
    }

    /**
     * Plays some effects to give feedback that the enemy has taken damage.
     *
     * Currently, it flashes the enemy red for the duration of its invincibility after being hit.
     * It also plays a hit sound, when present
     */
    private void PlayDamageEffects()
    {
        // Flash enemy red for the duration of its temporary invincibility
        _tintFlashController.FlashTint(
            Color.red, TimeInvincible
        );
        
        // Play sound
        if (!ReferenceEquals(hitSounds, null))
        {
            hitSounds.PlayOneShot();
        }
    }
}
