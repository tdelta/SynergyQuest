using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewDirection{
    top, 
    bottom,
    left,
    right
}

public enum PlayerState{
    walking,
    attacking,
    // The pulling state also models the pushing of objects. TODO: better name?
    pulling
}

public class PlayerController : EntityController
{
    [SerializeField] private GameObject lifeGauge;
    [SerializeField] private float speed = 3.0f; // units per second
    [SerializeField] private int maxHealthPoints = 5;
    [SerializeField] private float boxPullRange;
    
    private int _healthPoints;

    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    
    private BoxController _boxToPull;
    private float _vertical;
    private float _horizontal;
    /**
     *Modeling the current action of the player
     */
    private PlayerState _playerState;
    
    /**
     * TODO: To be discussed (from Marc) : Do we need this variable as an class attribute?
     */
    private Vector2 _lookDirection = new Vector2(1,0);
    
    /**
     * Set by the animator, makes it easier to implement logic which depends on viewDirection

     * Additional note (from Marc) : Technically we could calculate the viewDirection by the variable above (lookDirection),
     * but this is a tideous task and can be done within the animator. Maybe remove attribute above (Or keep for debug purpose?).
     */
    public ViewDirection viewDirection;
    
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
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _playerState = PlayerState.walking;
    }

    // Update is called once per frame
    void Update()
    {
        // Check whether the player released the pull key
        if (!Input.GetKey(KeyCode.P) && _playerState == PlayerState.pulling){
            ReleasePull();
        }

        // Attacking
        if (Input.GetKeyDown(KeyCode.Space) && _playerState != PlayerState.pulling) {
            _playerState = PlayerState.attacking;
            Attack();
        }   
        
        // Pulling / Pushing
        else if (Input.GetKey(KeyCode.P) && _playerState == PlayerState.walking){
            BoxController nearBox = IsMatchingBoxNear();
            // Check whether there is something to pull
            if(nearBox != null) {
                _playerState = PlayerState.pulling;
                Pull(nearBox);
            }
        }
    }

    void FixedUpdate ()
    {
        // If the player is walking normally, he is able to move vertically and horizontally
        if(this._playerState == PlayerState.walking) {
            Move(true, true);
        } 
        // If the player is pulling a box, he is only able to walking vertically or horizontally
        // depending on is viewDirection
        else if (this._playerState == PlayerState.pulling) {
            switch(viewDirection) {
                case ViewDirection.top:
                case ViewDirection.bottom:
                    Move(true, false);
                    break;
                case ViewDirection.left:
                case ViewDirection.right:
                    Move(false, true);
                    break;
            }
        }
    }

    public override void PutDamage(int amount, Vector2 knockbackDirection)  {
        var stopForce = -_rigidbody2D.velocity * _rigidbody2D.mass;
        _rigidbody2D.AddForce(stopForce + knockbackFactor * amount * knockbackDirection, ForceMode2D.Impulse);
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
        return _rigidbody2D.position;
    }

    private void Attack()
    {
        _animator.SetTrigger(AttackTrigger);
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        // After attack aninmation is done (0.3 seconds), the playerstate changes back to walking
        _playerState = PlayerState.walking;
    }

    /*
    Implements the moving logic
    */
    private void Move(bool enableVertical, bool enableHorizontal)
    {
        _vertical = (enableVertical) ? Input.GetAxis("Vertical") : 0;
        _horizontal = (enableHorizontal) ? Input.GetAxis("Horizontal") : 0;

        // Scale movement speed by the input axis value and the passed time to get a delta which must be applied to the current position
        Vector2 deltaPosition = new Vector2(
            _horizontal,
            _vertical
        ) * (speed * Time.deltaTime);

        if (!Mathf.Approximately(deltaPosition.x, 0.0f) || !Mathf.Approximately(deltaPosition.y, 0.0f)) {
            _lookDirection.Set(deltaPosition.x, deltaPosition.y);
            _lookDirection.Normalize();
        }

        _animator.SetFloat(LookXProperty, _lookDirection.x);
        _animator.SetFloat(LookYProperty, _lookDirection.y);
        _animator.SetFloat(SpeedProperty, deltaPosition.magnitude);
        
        _rigidbody2D.MovePosition(
            _rigidbody2D.position + deltaPosition
        );
    }

    /**
     *Checks whether there is a pullable box near to the player and returns 
     *it if there.
     *
     *TODO: Ignores color currently, due to having only one player
     */
    private BoxController IsMatchingBoxNear()
    {
        Vector2 v;        
        switch (viewDirection) {
            case ViewDirection.top:
                v = new Vector2(0,1);
                break;
            case ViewDirection.bottom:
                v = new Vector2(0,-1);
                break;
            case ViewDirection.left:
                v = new Vector2(-1,0);
                break;
            //case ViewDirection.left:
            default:
                v = new Vector2(1,0);
                break;
        }
        RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, v, boxPullRange, LayerMask.GetMask("Box"));

        if (hit.collider != null) {
            return hit.collider.gameObject.GetComponent<BoxController>();
        } else {
            return null;
        }
    }

    private void Pull(BoxController box)
    {
        box.GetComponent<FixedJoint2D>().connectedBody = this.GetComponent<Rigidbody2D>();
        _boxToPull = box;
    }

    private void ReleasePull()
    {
        _boxToPull.GetComponent<FixedJoint2D>().connectedBody = null;
        _boxToPull = null;
        _playerState = PlayerState.walking; 
    }
}
