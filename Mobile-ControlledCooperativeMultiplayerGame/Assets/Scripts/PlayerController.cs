using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController
{
    [SerializeField] private GameObject lifeGauge;
    [SerializeField] private int maxHealthPoints = 5;

    public float speed = 3.0f;
    private int _healthPoints;

    Animator animator;
    new Rigidbody2D rigidbody2D;


    float vertical;
    float horizontal;
    Vector2 lookDirection = new Vector2(1,0);
    bool attacking = false;

    void Awake()
    {
        _healthPoints = maxHealthPoints;
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            attack();
        }   
    }

    void FixedUpdate ()
    {
        if(attacking == false) {
            move();
        }
    }

    public override void putDamage(int amount, Vector2 knockbackDirection)  {
        var stopForce = -rigidbody2D.velocity * rigidbody2D.mass;
        rigidbody2D.AddForce(stopForce + knockbackFactor * amount * knockbackDirection, ForceMode2D.Impulse);
        ChangeHealth(-amount);
    }

    private void ChangeHealth(int delta)
    {
        _healthPoints += delta;

        if (delta != 0)
        {
            DisplayLifeGauge();
        }
        
        if (_healthPoints <= 0) {
            Destroy(this.gameObject);
        }
    }

    private void DisplayLifeGauge()
    {
        var spriteBounds = this.GetComponent<SpriteRenderer>().bounds.size;
        this.lifeGauge.transform.localPosition =
            new Vector3(
                0,
                spriteBounds.y * 0.7f,
                0
            );
        
        this.lifeGauge.GetComponent<LifeGauge>().DrawLifeGauge(_healthPoints, maxHealthPoints);
    }

    public Vector2 getPosition()
    {
        return rigidbody2D.position;
    }

    private void attack()
    {
        animator.SetTrigger("Attack");
        StartCoroutine(attackCoroutine());
    }

    private IEnumerator attackCoroutine()
    {
        attacking = true;
        yield return new WaitForSeconds(0.3f);
        attacking = false;
    }

    private void move()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f)) {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look x", lookDirection.x);
        animator.SetFloat("Look y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);


        Vector2 newPosition = rigidbody2D.position;
        newPosition.x = horizontal * speed;
        newPosition.y = vertical * speed;

        rigidbody2D.AddForce(newPosition);
    }
}
