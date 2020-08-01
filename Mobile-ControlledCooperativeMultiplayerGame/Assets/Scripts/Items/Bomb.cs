using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] ParticleSystem sparkEffect;

    bool destroyed = false;
    bool explosion = false;
    readonly int explosionTrigger = Animator.StringToHash("Explode");

    private Animator _animator;
    private PhysicsEffects _effects;
    private Rigidbody2D _rigidbody2D;
    private AudioSource _audioSource;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _effects = GetComponent<PhysicsEffects>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _animator.SetTrigger(explosionTrigger);
    }

    public void Explode()
    {
        _audioSource.Play();
        sparkEffect.Stop();
        sparkEffect.GetComponent<AudioSource>().Stop();
        explosion = true;
    }

    public void Destroy()
    {
        // new bomb can only be instantiated when the previous one exploded
        destroyed = true;
        Destroy(gameObject);
    }

    public bool isDestroyed()
    {
      return destroyed;
    }
    
    public Bomb Instantiate(Vector2 coords)
    {
      Bomb instance = Instantiate(this, coords, Quaternion.identity);
      instance.gameObject.SetActive(true);
      return instance;
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
