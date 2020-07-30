using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombItem : Item, Throwable
{
    [SerializeField] ParticleSystem sparkEffect;

    bool destroyed = false;
    bool explosion = false;
    readonly Vector3 scaleFactor = new Vector3(0.01f, 0.01f, 0.01f);
    readonly int explosionTrigger = Animator.StringToHash("Explode");

    BombItem instance;
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

    public IEnumerator PickUpCoroutine(Vector2 ontop, HingeJoint2D joint, BoxCollider2D otherCollider)
    {
        joint.connectedBody = rigidbody2D;
        animator.SetTrigger(explosionTrigger);
        Physics2D.IgnoreCollision(collider, otherCollider);
        // temporally change sorting order to draw carried gameobject on top
        renderer.sortingOrder++;
        effects.MoveBody(ontop);

        // the joint should be disabled until the carried object moved ontop of the carrying player,
        // because a joint disallows such movements
        yield return new WaitForFixedUpdate(); 
        joint.enabled = true;
        
        // if the bomb explodes before we throw it, make sure to get damage
        yield return new WaitUntil(() => explosion);
        Physics2D.IgnoreCollision(collider, otherCollider, false);
        Destroy(joint);
    }

    public IEnumerator ThrowCoroutine(Vector2 direction, BoxCollider2D otherCollider)
    {
        // can't throw a bomb if already exploded
        if (destroyed)
            yield break;

        effects.ApplyImpulse(10 * direction);

        // according to unity manual equality checks on vectors take floating point inaccuracies into account
        // https://docs.unity3d.com/ScriptReference/Vector2-operator_eq.html
        yield return new WaitUntil(() => effects.GetImpulse() == Vector2.zero);
        Physics2D.IgnoreCollision(collider, otherCollider, false);
        renderer.sortingOrder--;
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

    protected bool isDestroyed()
    {
      return destroyed;
    }
    
    // FIXME: this method belongs in a new class InventoryItem
    public override bool Ready()
    {
        return instance?.isDestroyed() ?? true;
    }

    // FIXME: this method belongs in a new class InventoryItem
    public override Item Instantiate(Vector2 coords)
    {
      instance = Instantiate(this, coords, Quaternion.identity);
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

    public override void Shrink()
    {
        if (effects.GetImpulse() == Vector2.zero)
        {
          gameObject.transform.localScale -= scaleFactor;
        }
    }
}
