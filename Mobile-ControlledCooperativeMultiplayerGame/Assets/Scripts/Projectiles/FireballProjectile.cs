using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] int thrust = 1;
    [SerializeField] int damageFactor = 1;

    readonly int HitTrigger = Animator.StringToHash("Hit");

    Vector2 _direction = Vector2.zero;
    Animator _animator;
    Rigidbody2D _rigidbody2D;
    PhysicsEffects _physicsEffects;

    public BoxCollider2D Collider { get; private set; }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
        Collider = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        _physicsEffects.MoveBody(_rigidbody2D.position + Time.deltaTime * thrust * _direction);
    }

    public void Launch(Vector2 direction)
    {
        // rotate projectile into direction of flight
        transform.up = -direction;
        _direction = direction;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<EntityController>();
            player.PutDamage(damageFactor, (other.transform.position - transform.position).normalized); 
        }
        _animator.SetTrigger(HitTrigger);
    }

    void OnHitAnimationComplete()
    {
        Destroy(gameObject);
    }
}
