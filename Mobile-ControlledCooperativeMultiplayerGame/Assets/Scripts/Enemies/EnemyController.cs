using UnityEngine;

abstract public class EnemyController : EntityController
{
    [SerializeField] protected int   healthPoints = 1;
    [SerializeField] protected float directionSpeed = 1;
    [SerializeField] protected float directionChangeTime = 1;
    [SerializeField] protected int damageFactor = 1;
    [SerializeField] ParticleSystem smokeEffect;
    [SerializeField] private MultiSound hitSounds;

    public GameObject coin;

    protected float directionTimer;
    protected Vector2 direction;
    
    bool isDead;
    readonly int deadTrigger = Animator.StringToHash("Dead");

    /**
     * Used to briefly flash an enemy in a certain color. For example red when it is hit.
     */
    private TintFlashController _tintFlashController;

    /**
     * Used to notify other game elements (door) when a monster dies
     */
    public delegate void EnemyDied();
    public static event EnemyDied OnDeath;

    protected override void Start()
    {
        base.Start();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
        
        _tintFlashController = GetComponent<TintFlashController>();
    }

    protected override void Update()
    {
        base.Update();
        directionTimer -= Time.deltaTime;

        if (directionTimer < 0)
        {
            direction = Random.insideUnitCircle.normalized;
            directionTimer = directionChangeTime;
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<EntityController>();
            player.PutDamage(damageFactor, (other.transform.position - transform.position).normalized); 
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player") && !!other.gameObject.CompareTag("PlayerHit"))
            direction = -direction;
    }

    protected override bool ChangeHealth(int amount, bool playSounds = true)
    {
        healthPoints += amount;

        if (amount <= 0)
        {
            PlayDamageEffects();
        }

        if (healthPoints <= 0)
        {
            isDead = true;
            this.GetComponent<Collider2D>().enabled = false;
            Animator.SetTrigger(deadTrigger);
            dropCoins();
            Destroy(gameObject, 1);
            if(OnDeath != null)
            {
                OnDeath();
            }
        }
        return true;
    }

    /*
     * Different enemy types only differ in their movements. New enemies should implement this method,
     * to define their behaviour.
     */
    protected abstract Vector2 ComputeOffset();
    
    void FixedUpdate()
    {
        if (!isDead)
        {
            Vector2 position = Rigidbody2D.position;
            position += ComputeOffset();
            
            PhysicsEffects.MoveBody(position);
        }
    }

    public void ShowParticles()
    {
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

    private void dropCoins()
    {
        int amountCoins = Random.Range(0,5);
        for(int i = 0; i < amountCoins; i++) {
            Instantiate(coin, transform.position, Quaternion.identity);    
        }
    }

}
