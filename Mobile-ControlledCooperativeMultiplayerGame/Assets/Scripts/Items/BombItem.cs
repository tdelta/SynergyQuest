using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombItem : Item, Throwable
{
    readonly int explosionTrigger = Animator.StringToHash("Explode");
    Renderer renderer;
    Animator animator;
    PhysicsEffects effects;
    BoxCollider2D collider;
    Rigidbody2D rigidbody2D;

    // Start is called before the first frame update
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        renderer = GetComponent<Renderer>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        effects = new PhysicsEffects(rigidbody2D);  
        collider = GetComponent<BoxCollider2D>();
    }

    public IEnumerator PickUpCoroutine(Vector2 ontop, HingeJoint2D joint, BoxCollider2D otherCollider)
    {
        joint.connectedBody = rigidbody2D;
        Debug.Log(animator);
        animator.SetTrigger(explosionTrigger);
        Physics2D.IgnoreCollision(collider, otherCollider);
        // temporally change sorting order to draw carried gameobject on top
        renderer.sortingOrder++;
        effects.MoveBody(ontop);

        // the joint should be disabled until the carried object moved ontop of the carrying player,
        // because a joint disallows such movements
        yield return new WaitForFixedUpdate(); 
        joint.enabled = true;

    }

    public IEnumerator ThrowCoroutine(Vector2 direction, BoxCollider2D otherCollider)
    {
        effects.ApplyImpulse(10 * direction);

        // according to unity manual equality checks on vectors take floating point inaccuracies into account
        // https://docs.unity3d.com/ScriptReference/Vector2-operator_eq.html
        yield return new WaitUntil(() => effects.GetImpulse() == Vector2.zero);
        Physics2D.IgnoreCollision(collider, otherCollider, false);
        renderer.sortingOrder--;

    }
}
