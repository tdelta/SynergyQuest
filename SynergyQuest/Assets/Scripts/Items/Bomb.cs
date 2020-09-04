using UnityEngine;

public class Bomb : Item
{
    [SerializeField] ParticleSystem sparkEffect = default;

    bool explosion = false;
    readonly int explosionTrigger = Animator.StringToHash("Explode");

    private Animator _animator;
    private PhysicsEffects _effects;
    private Rigidbody2D _rigidbody2D;
    private AudioSource _audioSource;
    private Throwable _throwable;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _effects = GetComponent<PhysicsEffects>();
        _audioSource = GetComponent<AudioSource>();
        _throwable = GetComponent<Throwable>();
    }

    private void Start()
    {
        _animator.SetTrigger(explosionTrigger);
    }
    
    protected override void OnActivate(PlayerController player)
    {
        _throwable.Pickup(player);
    }

    /**
     * Invoked by the animation controller when the bomb shall start exploding
     */
    public void Explode()
    {
        _audioSource.Play();
        sparkEffect.Stop();
        sparkEffect.GetComponent<AudioSource>().Stop();
        explosion = true;

        // Checks whether the bomb is still carried when it explodes. If so, damage the carrier
        if (_throwable.IsBeingCarried) {
            _throwable.Carrier.PutDamage(1, (_throwable.Carrier.transform.position - transform.position).normalized);
        }

    }

    /**
     * Invoked by animation after bomb exploded
     */
    public void Destroy()
    {
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (!explosion)
            _effects.MoveBody(_rigidbody2D.position);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (explosion) 
        {
            var otherGameobject = other.collider.gameObject;
            if (otherGameobject.CompareTag("Enemy") || otherGameobject.CompareTag("Player"))
            {
                var entity = other.gameObject.GetComponent<EntityController>();
                entity.PutDamage(1, (other.transform.position - transform.position).normalized); 
            }
            else if (otherGameobject.CompareTag("Switch"))
                otherGameobject.GetComponent<ShockSwitch>().Activate();
            else if (otherGameobject.CompareTag("DestroyableWall"))
                Destroy(otherGameobject);
            else if (other.gameObject.CompareTag("Ghost"))
                other.gameObject.GetComponent<PlayerGhost>().Exorcise();
        }
    }
    
}
