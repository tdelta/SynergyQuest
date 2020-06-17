using System.Collections;
using UnityEngine;

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
    [SerializeField] private MultiSound fightingSounds;
    [SerializeField] private MultiSound hitSounds;
    [SerializeField] private MultiSound deathSounds;
    /**
     * If local controls will be used for this character instead of a remote controller, which color should be assigned
     * to this player?
     * Has no effect if remote controls are used.
     */
    [SerializeField] private PlayerColor localControlsInitColor = PlayerColor.Any;
    /**
     * If local controls will be used for this character instead of a remote controller, which keyboard layout shall
     * be used for them?
     * Has no effect if remote controls are used.
     */
    [SerializeField] private LocalKeyboardLayout localDefaultLayout = LocalKeyboardLayout.WASD;
    
    private int _healthPoints;

    private BoxCollider2D _collider;
    
    /**
     * Used to briefly flash the player in a certain color. For example red when they are hit.
     */
    private TintFlashController _tintFlashController;

    /**
     * Initialize input to local. However, it may be reassigned in the Init method to a remote controller, see
     * also `ControllerInput`.
     */
    private Input _input;
    
    private float _vertical;
    private float _horizontal;
    
    private Pushable _pushableToPull;
    
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
     * but this is a tedious task and can be done within the animator. Maybe remove attribute above (Or keep for debug purpose?).
     */
    public Direction viewDirection;
    
    /**
     * Caching animation property identifiers and triggers as hashes for better performance
     */
    private static readonly int LookXProperty = Animator.StringToHash("Look x");
    private static readonly int LookYProperty = Animator.StringToHash("Look y");
    private static readonly int SpeedProperty = Animator.StringToHash("Speed");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");

    public PlayerColor Color => _input.GetColor();

    /**
     * Should be used to assign a remote controller to this player after creating the game object instance from a
     * prefab using `Instantiate`.
     *
     * If this method is not called before the first frame, local input will be used instead.
     */
    public void Init(Input input)
    {
        _input = input;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        // If `Init` has not been called and no remote input has been assigned, we assign a local input controller
        // instead
        if (_input == null)
        {
            _input = new LocalInput(localDefaultLayout, localControlsInitColor);
        }
        
        _collider = GetComponent<BoxCollider2D>();
        _tintFlashController = GetComponent<TintFlashController>();
        
        _healthPoints = maxHealthPoints;
        _playerState = PlayerState.walking;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // Check whether the player released the pull key
        if (!_input.GetButton(Button.Pull) && _playerState == PlayerState.pulling){
            ReleasePull();
        }

        // Attacking
        if (_input.GetButtonDown(Button.Attack) && _playerState != PlayerState.pulling) {
            _playerState = PlayerState.attacking;
            Attack();
        }   
        
        // Pulling / Pushing
        else if (_input.GetButtonDown(Button.Pull) && _playerState == PlayerState.walking)
        {
            if (GetNearPushable(out var pushable))
            {
                EnablePulling(pushable);
            }
        }
    }

    /**
     * Checks whether there is a pullable object near to the player in viewing direction.
     *
     * @param pushable    if a pullabls object has been found, it will be stored here, otherwise, null is stored
     * @returns true iff a pullable object has been found.
     * 
     * FIXME: Check color of box
     */
    bool GetNearPushable(out Pushable pushable)
    {
        // We want to search in viewing direction
        var searchDirection = viewDirection.ToVector();
        var hit = Physics2D.Raycast(
            (Vector2) transform.position + _collider.offset,  // Search from middle point of our collider
            searchDirection,
            boxPullRange,
            LayerMask.GetMask("Box")
        );

        if (!ReferenceEquals(hit.collider, null)) // !ReferenceEquals is supposed to be faster than != null
        {
            pushable = hit.collider.gameObject.GetComponent<Pushable>();

            if (!ReferenceEquals(pushable, null))
            {
                // We can only interact with boxes where the color matches our own
                return pushable.Color.IsCompatibleWith(this.Color);
            }            
        }
    
        pushable = null;
        return false;
    }

    void FixedUpdate ()
    {
        // If the player is walking normally, they are able to move vertically and horizontally
        if (_playerState == PlayerState.walking)
        {
            Move(true, true);
        } 
        
        // If the player is pulling a box, they are only able to walk vertically or horizontally
        // depending on the viewDirection
        else if (_playerState == PlayerState.pulling)
        {
            switch(viewDirection) {
                case Direction.Up:
                case Direction.Down:
                    Move(true, false);
                    break;
                case Direction.Left:
                case Direction.Right:
                    Move(false, true);
                    break;
            }
        }
        else {
            // Prevent change of velocity by collision forces
            Move(false, false);
        }
    }

    protected override void ChangeHealth(int delta)
    {
        _healthPoints += delta;

        // Display some effects when damaged
        if (delta < 0)
        {
            _tintFlashController.FlashTint(UnityEngine.Color.red, TimeInvincible);
            hitSounds.PlayOneShot();
        }

        if (delta != 0)
        {
            DisplayLifeGauge();
        }
        
        if (_healthPoints <= 0) {
            deathSounds.PlayOneShot();
            
            // This is only a temporary solution until we have respawn
            rigidbody2D.simulated = false;
            lifeGauge.SetActive(false);
            GetComponent<SpriteRenderer>().enabled = false;
            Destroy(this.gameObject, 2.0f);
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
        fightingSounds.PlayOneShot();
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.3f);
        // After attack aninmation is done (0.3 seconds), the playerstate changes back to walking
        _playerState = PlayerState.walking;
    }

    /**
     * Assuming we are pulling a box, this method determines whether the inputs letting the player move in the direction
     * of pulling.
     */
    private bool DoesMoveInPullDirection()
    {
        switch (viewDirection)
        {
            case Direction.Left:
                return _horizontal > 0;
            case Direction.Right:
                return _horizontal < 0;
            case Direction.Down:
                return _vertical > 0;
            case Direction.Up:
                return _vertical < 0;
        }

        return false;
    }

    /**
     * Implements the moving logic
     */
    private void Move(bool enableVertical, bool enableHorizontal)
    {
        _vertical = (enableVertical) ? _input.GetVertical() : 0;
        _horizontal = (enableHorizontal) ? _input.GetHorizontal() : 0;

        // If we are pulling a box and trying to move in the pulling direction, we instruct the box to pull
        if (_playerState == PlayerState.pulling && DoesMoveInPullDirection())
        {
            _pushableToPull.Pull(viewDirection.Inverse(), this.gameObject);
        }

        // Otherwise, move normally
        else
        {
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
            
            effects.MoveBody(
                rigidbody2D.position + deltaPosition
            );
        }
    }

    
    private void EnablePulling(Pushable pushable)
    {
        _playerState = PlayerState.pulling;
        _pushableToPull = pushable;
    }

    private void ReleasePull()
    {
        _playerState = PlayerState.walking;
        _pushableToPull = null;
    }
}
