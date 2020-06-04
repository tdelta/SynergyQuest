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
    [SerializeField] private int maxHealthPoints = 5;

    public float speed = 3.0f;
    private int _healthPoints;

    Animator animator;
    new Rigidbody2D rigidbody2D;


    float vertical;
    float horizontal;

    public float boxPullRange;
    BoxController boxToPull;

    /*
    To be discussed (from Marc) : Do we need this variable as an class attribute?
    */
    Vector2 lookDirection = new Vector2(1,0);
    
    /*
    Set by the animator, makes it easier to implement logic which depends on viewDirection

    Additional note (from Marc) : Technically we could calculate the viewDirection by the variable above (lookDirection),
    but this is a tideous task and can be done within the animator. Maybe remove attribute above (Or keep for debug purpose?).
    */
    public ViewDirection viewDirection;

    /*
    Modeling the current action of the player
    */
    PlayerState playerState;

    void Awake()
    {
        _healthPoints = maxHealthPoints;
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerState = PlayerState.walking;
    }

    // Update is called once per frame
    void Update()
    {
        // Check whether the player released the pull key
        if (!Input.GetKey(KeyCode.P) && playerState == PlayerState.pulling){
            releasePull();
        }

        // Attacking
        if (Input.GetKeyDown(KeyCode.Space) && playerState != PlayerState.pulling) {
            playerState = PlayerState.attacking;
            attack();
        }   
        // Pulling / Pushing
        else if (Input.GetKey(KeyCode.P) && playerState == PlayerState.walking){
            BoxController nearBox = isMatchingBoxNear();
            // Check whether there is something to pull
            if(nearBox != null) {
                playerState = PlayerState.pulling;
                pull(nearBox);
            }
        }
    }

    void FixedUpdate ()
    {
        // If the player is walking normally, he is able to move vertically and horizontally
        if(this.playerState == PlayerState.walking) {
            move(true, true);
        } 
        // If the player is pulling a box, he is only able to walking vertically or horizontally
        // depending on is viewDirection
        else if (this.playerState == PlayerState.pulling) {
            switch(viewDirection) {
                case ViewDirection.top:
                case ViewDirection.bottom:
                    move(true, false);
                    break;
                case ViewDirection.left:
                case ViewDirection.right:
                    move(false, true);
                    break;
            }
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
        yield return new WaitForSeconds(0.3f);
        // After attack aninmation is done (0.3 seconds), the playerstate changes back to walking
        playerState = PlayerState.walking;
    }

    /*
    Implements the moving logic
    */
    private void move(bool enableVertical, bool enableHorizontal)
    {
        vertical = (enableVertical) ? Input.GetAxis("Vertical") : 0;
        horizontal = (enableHorizontal) ? Input.GetAxis("Horizontal") : 0;

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

    /*
    Checks whether there is a pullable box near to the player and returns 
    it if there.

    TODO: Ignores color currently, due to having only one player
    */
    private BoxController isMatchingBoxNear()
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

    private void pull(BoxController box)
    {
        box.GetComponent<FixedJoint2D>().connectedBody = this.GetComponent<Rigidbody2D>();
        boxToPull = box;
    }

    private void releasePull()
    {
        boxToPull.GetComponent<FixedJoint2D>().connectedBody = null;
        boxToPull = null;
        playerState = PlayerState.walking; 
    }
}
