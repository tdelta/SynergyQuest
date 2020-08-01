using UnityEngine;

public class Bomb : Item
{
    [SerializeField] ParticleSystem sparkEffect;

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
    }

    /**
     * Invoked by animation after bomb exploded
     */
    public void Destroy()
    {
        Destroy(gameObject);
    }
    
    private void Update()
    {
        var carrier = _throwable.Carrier;
        
        // If we are currently being carried, we shall be thrown once the player releases the item button
        if (_throwable.IsBeingCarried && carrier.Input.GetButtonUp(ItemDescription.UseButton))
        {
            _throwable.Carrier.ThrowThrowable(_throwable, carrier.ThrowingDirection);
        }
    }

    void FixedUpdate()
    {
        if (_effects.GetImpulse() != Vector2.zero && !explosion)
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
        }
    }
    
}
