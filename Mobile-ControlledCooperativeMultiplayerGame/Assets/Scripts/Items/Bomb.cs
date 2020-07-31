using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] ParticleSystem sparkEffect;

    bool destroyed = false;
    bool explosion = false;
    readonly int explosionTrigger = Animator.StringToHash("Explode");

    Renderer renderer;
    Animator animator;
    PhysicsEffects effects;
    BoxCollider2D collider;
    Rigidbody2D rigidbody2D;

    void Awake()
    {
        animator = GetComponent<Animator>();
        renderer = GetComponent<Renderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        effects = GetComponent<PhysicsEffects>();
        collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        animator.SetTrigger(explosionTrigger);
    }

    public void Explode()
    {
        sparkEffect.Stop();
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
        if (effects.GetImpulse() != Vector2.zero && !explosion)
            effects.MoveBody(rigidbody2D.position);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (explosion && (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player")))
        {
            var entity = other.gameObject.GetComponent<EntityController>();
            entity.PutDamage(1, (other.transform.position - transform.position).normalized); 
        }
        else if (explosion && other.gameObject.CompareTag("Switch"))
            other.gameObject.GetComponent<ShockSwitch>().Activate();
        else if (explosion && other.gameObject.CompareTag("DestroyableWall"))
            Destroy(other.gameObject);
    }
    
}
