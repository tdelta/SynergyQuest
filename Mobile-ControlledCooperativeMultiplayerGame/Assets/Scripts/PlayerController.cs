using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState{
    walking,
    attacking,
    // The pulling state also models the pushing of objects. TODO: better name?
    pulling,
    carrying,
    carried,
    thrown,
    falling,
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
    [SerializeField] private MultiSound fallingSounds;
    
    // FIXME: Remove this, just for testing purposes regarding the custom origin functionality of the `PhysicsEffects` class
    [SerializeField] private Transform customOrigin;
    
    /**
     * If local controls will be used for this character instead of a remote controller, which color should be assigned
     * to this player?
     * Has no effect if remote controls are used.
     */
    [SerializeField] private PlayerColor localControlsInitColor = PlayerColor.Any;

    /**
     * If local controls will be used for this character instead of a remote controller, which local keyboard layout
     * shall be used for them?
     * Has no effect if remote controls are used.
     */
    [SerializeField] private LocalInput localInputPrefab;
    
    private int _healthPoints;

    private BoxCollider2D _collider;
    private Renderer _renderer;
    
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
     * Reference to the other player during carrying
     */
    private PlayerController _otherPlayer;
    
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
    private static readonly int CarryingState = Animator.StringToHash("Carrying");
    private static readonly int CarriedState  = Animator.StringToHash("Carried");
    private static readonly int FallTrigger = Animator.StringToHash("Fall");

    public PlayerColor Color => _input.GetColor();

    /**
     * Delegate function is filled by the script that does the respawning
     */
    public delegate void OnRespawnAction(PlayerController player);
    public event OnRespawnAction OnRespawn;

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
            var localInput = Instantiate(localInputPrefab, this.transform);
            localInput.SetColor(localControlsInitColor);

            _input = localInput;
        }
        _input.OnMenuActionTriggered += OnMenuActionTriggered;
        
        _collider = GetComponent<BoxCollider2D>();
        _tintFlashController = GetComponent<TintFlashController>();
        _renderer = GetComponent<Renderer>();
        
        _healthPoints = maxHealthPoints;
        _playerState = PlayerState.walking;
        
        var material = GetComponent<Renderer>().material;
        material.SetColor("_ShirtColor", PlayerColorMethods.ColorToRGB(this.Color));
        
        
        // FIXME: Remove this, just for testing purposes regarding the custom origin functionality of the `PhysicsEffects` class
        if (customOrigin != null)
        {
            PhysicsEffects.SetCustomOrigin(customOrigin);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // Check whether the player released the pull key
        if (!_input.GetButton(Button.Pull) && _playerState == PlayerState.pulling)
            ReleasePull();
        // Check whether the player released the carry key
        else if (!_input.GetButton(Button.Carry) && _playerState == PlayerState.carrying)
            ThrowPlayer(new Vector2(_input.GetHorizontal(), _input.GetVertical()));

        // Attacking
        if (_input.GetButtonDown(Button.Attack) && _playerState != PlayerState.pulling && 
        _playerState != PlayerState.carrying && _playerState != PlayerState.carried) {
            _playerState = PlayerState.attacking;
            Attack();
        }   
        // Pulling / Pushing
        else if (_input.GetButtonDown(Button.Pull) && _playerState == PlayerState.walking &&
        GetNearPushable(out var pushable))
            EnablePulling(pushable);
        // Carrying
        else if (_input.GetButtonDown(Button.Carry) && _playerState == PlayerState.walking &&
        GetNearPlayer(out var otherPlayer))
                PickUpPlayer(otherPlayer);
    }

    private void OnDestroy()
    {
        if (_input != null)
        {
            // Unregister callbacks when destroyed
            _input.OnMenuActionTriggered -= OnMenuActionTriggered;
        }
    }

    /**
     * Callback, which is called, should a remote controller send a menu action, e.g MenuAction.PauseGame etc.
     *
     * Note, that some menu actions are handled by other behaviors, e.g. `MenuAction.StartGame` is handled by
     * `LobbyMenuUi`.
     */
    private void OnMenuActionTriggered(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.PauseGame:
                PauseScreenLauncher.Instance.Launch();
                break;
            case MenuAction.ResumeGame:
                if (PauseScreenLauncher.Instance.IsPaused)
                {
                    PauseScreenLauncher.Instance.Close();
                }

                else if (InfoScreenLauncher.Instance.IsShowingInfoScreen)
                {
                    InfoScreenLauncher.Instance.Close();
                }
                break;
            case MenuAction.Next:
                InfoScreenLauncher.Instance.NextPage();
                break;
            case MenuAction.Back:
                InfoScreenLauncher.Instance.PreviousPage();
                break;
        }
    }

    /**
     * Checks whether there is a pullable object near to the player in viewing direction.
     *
     * @param pushable    if a pullable object has been found, it will be stored here, otherwise, null is stored
     * @returns true iff a pullable object has been found.
     * 
     * FIXME: Check color of box
     */
    bool GetNearPushable(out Pushable pushable)
    {
        var hit = Search("Box");
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

    bool GetNearPlayer(out PlayerController player)
    {
        var hit = Search("Player");
        if (hit.collider?.gameObject.GetComponent<PlayerController>() is PlayerController candidate && !candidate.CarriesSomeone())
        {
            player = candidate;
            return true;
        }
    
        player = null;
        return false;
    }

    RaycastHit2D Search(string layer)
    {
        // We want to search in viewing direction
        var searchDirection = viewDirection.ToVector();
        return Physics2D.Raycast(
            (Vector2) transform.position + _collider.offset,  // Search from middle point of our collider
            searchDirection,
            boxPullRange,
            LayerMask.GetMask(layer));
    }

    void FixedUpdate ()
    {
        switch (_playerState) {
            // If the player is walking normally, they are able to move vertically and horizontally
            case PlayerState.walking:
            case PlayerState.carrying:   
                Move(true, true);
                break;
            // If the player is pulling a box, they are only able to walk vertically or horizontally
            // depending on the viewDirection
            case PlayerState.pulling:
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
                break;
            // because of joint carried player shouldn't call Move
            case PlayerState.carried: 
                break;
            // according to unity manual equality checks on vectors take floating point inaccuracies into account
            // https://docs.unity3d.com/ScriptReference/Vector2-operator_eq.html
            case PlayerState.thrown when PhysicsEffects.GetImpulse() == Vector2.zero:
                _animator.SetBool(CarriedState, false);
                _playerState = PlayerState.walking;
                break;
            // Prevent change of velocity by collision forces
            default:
                Move(false, false);
                break;
            }
    }

    public void Reset()
    {
        // if the player carries another player, release
        if (CarriesSomeone())
            ThrowPlayer(Vector2.zero);
        // if the player is carried by another player, release 
        else if (_otherPlayer?.CarriesSomeone() ?? false) {
            _otherPlayer.ThrowPlayer(Vector2.zero);
            _animator.SetBool(CarriedState, false);
            _playerState = PlayerState.walking;
        }
        ChangeHealth(maxHealthPoints);
    }

    protected override bool ChangeHealth(int delta, bool playSounds = true)
    {
        // if the player is thrown he shouldn't get any damage
        if (_playerState == PlayerState.thrown)
            return false;

        _healthPoints += delta;

        if (_healthPoints <= 0) {
            if (playSounds)
            {
                deathSounds.PlayOneShot();
            }
            OnRespawn?.Invoke(this);
        }
        
        // Display some effects when damaged
        if (delta < 0)
        {
            _tintFlashController.FlashTint(UnityEngine.Color.red, TimeInvincible);
            if (playSounds)
            {
                hitSounds.PlayOneShot();
            }
            _input.PlayVibrationFeedback(new List<float>
            {
                200
            });
        }

        if (delta > 0)
        {
            _tintFlashController.FlashTint(UnityEngine.Color.green, 0.5f);
        }

        if (delta != 0)
        {
            DisplayLifeGauge();
        }
        return true;
    }

    private void Die(){
        // This is only a temporary solution until we have respawn
        Rigidbody2D.simulated = false;
        lifeGauge.SetActive(false);
        GetComponent<SpriteRenderer>().enabled = false;
        Destroy(this.gameObject, 2.0f);
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
        return Rigidbody2D.position;
    }

    private void Attack()
    {
        Animator.SetTrigger(AttackTrigger);
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

            Animator.SetFloat(LookXProperty, _lookDirection.x);
            Animator.SetFloat(LookYProperty, _lookDirection.y);
            Animator.SetFloat(SpeedProperty, deltaPosition.magnitude);
            
            PhysicsEffects.MoveBody(
                Rigidbody2D.position + deltaPosition
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

    /**
     * Returns true if the player currently carries someone else
     */
    public bool CarriesSomeone()
    {
        return gameObject.GetComponent("HingeJoint2D");
    }

    /**
     * Call this method to pickup another player
     */
    void PickUpPlayer(PlayerController otherPlayer) 
    {
        _otherPlayer = otherPlayer;
        // glue to players together
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        // Stops objects from continuing to collide and creating more joints
        joint.enableCollision = false; 
        joint.enabled = false;

        var ontop = new Vector2(_rigidbody2D.position.x, _renderer.bounds.max.y);
        StartCoroutine(_otherPlayer.PickUpCoroutine(ontop, joint, _collider, this));
        _playerState = PlayerState.carrying;
        _animator.SetBool(CarryingState, true);
    }

    /**
     * Call this method to throw the carried player into direction (or drop if direction is zero vector)
     */
    void ThrowPlayer(Vector2 direction)
    {
        _playerState = PlayerState.walking;
        _animator.SetBool(CarryingState, false);

        Destroy(gameObject.GetComponent("HingeJoint2D"));
        StartCoroutine(_otherPlayer.ThrowCoroutine(direction, _collider));
        _otherPlayer = null;
    }

    /**
     * This coroutine is called on the player that is being carried
     */
    public IEnumerator PickUpCoroutine(Vector2 ontop, HingeJoint2D joint, BoxCollider2D collider, PlayerController other)
    {
        _otherPlayer = other;
        joint.connectedBody = _rigidbody2D;
        _playerState = PlayerState.carried;

        _animator.SetBool(CarriedState, true);
        Physics2D.IgnoreCollision(collider, _collider);
        // temporally change sorting order to draw carried gameobject on top
        _renderer.sortingOrder++;
        PhysicsEffects.MoveBody(ontop);

        // the joint should be disabled until the carried player moved ontop of the carrying player,
        // because a joint disallows such movements
        yield return new WaitForFixedUpdate(); 
        joint.enabled = true;
    }

    /**
     * This coroutine is called when a carried player is dropped or thrown
     */
    public IEnumerator ThrowCoroutine(Vector2 direction, BoxCollider2D collider)
    {
        _otherPlayer = null;
        _playerState = PlayerState.thrown;
        PhysicsEffects.ApplyImpulse(10 * direction);

        // restore sorting order & collision between the two players, when player leaves state thrown
        yield return new WaitUntil(() => _playerState == PlayerState.walking);
        Physics2D.IgnoreCollision(collider, _collider, false);
        _renderer.sortingOrder--;
    }

    /**
     * A flying player should damage enemies
     */
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && _playerState == PlayerState.thrown)
        {
            var enemy = other.gameObject.GetComponent<EntityController>();
            enemy.PutDamage(1, (other.transform.position - transform.position).normalized); 
        }
    }

    /**
     * Call this to let the player fall
     * (play animation, sound and let player die)
     *
     * This method is for example used by the `Chasm` class
     */
    public void InitiateFall()
    {
        if (_playerState != PlayerState.falling)
        {
            _playerState = PlayerState.falling;
            // If the player is being moved by a platform, they should no longer be moved when falling
            PhysicsEffects.RemoveCustomOrigin();
            Animator.SetTrigger(FallTrigger);
        }
    }
    
    /**
     * Called as soon as the fall animation started by an animation event.
     */
    public void OnFallAnimationStarted()
    {
        fallingSounds.PlayOneShot();
    }

    /**
     * Called as soon as the fall animation ended by an animation event.
     */
    public void OnFallAnimationComplete()
    {
        _playerState = PlayerState.walking;
        
        // The falling animation scales the player down.
        // We need to scale it up again. This would be done by the animator automatically, however, this will only happen
        // after this method completed, which is to late, since OnExitTrigger2D functions will then not be invoked.
        // This can lead to bugs, for example when the player was standing on a platform which needs the trigger.
        this.transform.localScale = new Vector3(1, 1, 1);
        
        // Make the player invisible until respawn
        GetComponent<SpriteRenderer>().enabled = false;
        
        // Kill the player
        ChangeHealth(-_healthPoints, false);
    }
}
