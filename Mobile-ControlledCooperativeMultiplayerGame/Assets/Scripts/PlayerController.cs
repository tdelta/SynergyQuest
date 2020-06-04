using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : EntityController
{
    [SerializeField] private GameObject lifeGauge;
    [SerializeField] private float speed = 3.0f; // units per second
    [SerializeField] private int maxHealthPoints = 5;
    
    private int _healthPoints;

    Animator animator;
    Rigidbody2D rigidbody2D;

    private float _vertical;
    private float _horizontal;
    private Vector2 _lookDirection = new Vector2(1,0);
    private bool _attacking = false;
    
    /**
     * Caching animation property identifiers and triggers as hashes for better performance
     */
    private static readonly int LookXProperty = Animator.StringToHash("Look x");
    private static readonly int LookYProperty = Animator.StringToHash("Look y");
    private static readonly int SpeedProperty = Animator.StringToHash("Speed");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

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
            Attack();
        }   
    }

    void FixedUpdate ()
    {
        if(_attacking == false) {
            Move();
        }
    }

    public override void PutDamage(int amount, Vector2 knockbackDirection)  {
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

    /**
     * Displays a bar of hearts (life gauge) relative to the player avatar
     */
    private void DisplayLifeGauge()
    {
        var spriteBounds = this.GetComponent<SpriteRenderer>().bounds.size;
        // Set relative position of the life gauge so that it appears slightly above the player character
        this.lifeGauge.transform.localPosition =
            new Vector3(
                0,
                spriteBounds.y * 0.7f,
                0
            );
        
        this.lifeGauge.GetComponent<LifeGauge>().DrawLifeGauge(_healthPoints, maxHealthPoints);
    }

    public Vector2 GetPosition()
    {
        return rigidbody2D.position;
    }

    private void Attack()
    {
        animator.SetTrigger(AttackTrigger);
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        _attacking = true;
        yield return new WaitForSeconds(0.3f);
        _attacking = false;
    }

    private void Move()
    {
        _vertical = Input.GetAxis("Vertical");
        _horizontal = Input.GetAxis("Horizontal");

        // Scale movement speed by the input axis value and the passed time to get a delta which must be applied to the current position
        Vector2 deltaPosition = new Vector2(
            _horizontal,
            _vertical
        ) * (speed * Time.deltaTime);

        if (!Mathf.Approximately(deltaPosition.x, 0.0f) || !Mathf.Approximately(deltaPosition.y, 0.0f)) {
            _lookDirection.Set(deltaPosition.x, deltaPosition.y);
            _lookDirection.Normalize();
        }

        animator.SetFloat(LookXProperty, _lookDirection.x);
        animator.SetFloat(LookYProperty, _lookDirection.y);
        animator.SetFloat(SpeedProperty, deltaPosition.magnitude);
        
        rigidbody2D.MovePosition(
            rigidbody2D.position + deltaPosition
        );
    }
}
