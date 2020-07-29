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
    presenting_item
}

public class PlayerController : EntityController, Throwable
{
    [SerializeField] private GameObject lifeGauge;
    [SerializeField] private GameObject coinGauge;
    
    [SerializeField] private float speed = 3.0f; // units per second
    [SerializeField] private int maxHealthPoints = 5;
    [SerializeField] private float boxPullRange;
    [SerializeField] private MultiSound fightingSounds;
    [SerializeField] private MultiSound hitSounds;
    [SerializeField] private MultiSound deathSounds;
    [SerializeField] private MultiSound fallingSounds;
    [SerializeField] private InteractionSpeechBubble interactionSpeechBubble;

    public InteractionSpeechBubble InteractionSpeechBubble => interactionSpeechBubble;
    
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
    
    /**
     * There is an animation, in which a player can present an item.
     * The sprite of the item will be displayed by this renderer.
     */
    [SerializeField] private SpriteRenderer itemPresentation;
    
    private int _healthPoints;

    public BoxCollider2D Collider { get; private set; }

    private Renderer _renderer;
    
    /**
     * Used to briefly flash the player in a certain color. For example red when they are hit.
     */
    private TintFlashController _tintFlashController;

    private float _vertical;
    private float _horizontal;
    
    /**
     * Caches `Pushable` object, the player is pulling
     */
    private Pushable _pushableToPull;

    /**
     * All player properties which shall persist across scenes.
     */
    private PlayerData _data;
    public Input Input => _data.input;

    /**
     * If the item can be thrown, safe reference to instantiated item
     */
    private Throwable _throwableItemInstance;
    
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
    private static readonly int PullingState = Animator.StringToHash("Pulling");
    private static readonly int CarryingState = Animator.StringToHash("Carrying");
    private static readonly int CarriedState  = Animator.StringToHash("Carried");
    private static readonly int FallTrigger = Animator.StringToHash("Fall");
    private static readonly int PresentItemTrigger = Animator.StringToHash("PresentItem");

    public PlayerColor Color => Input.GetColor();

    /**
     * Delegate function is filled by the script that does the respawning
     */
    public delegate void OnRespawnAction(PlayerController player);
    public event OnRespawnAction OnRespawn;
    
    /**
     * There is an animation in which the player presents an item.
     * This callback will be invoked, when the animation completes.
     * See also the `PresentItem` method.
     */
    public delegate void PresentItemAction();
    private PresentItemAction _presentItemCallback;

    /**
     * Should be used to assign a remote controller to this player after creating the game object instance from a
     * prefab using `Instantiate`.
     *
     * If this method is not called before the first frame, local input will be used instead.
     */
    public void Init(PlayerData data)
    {
        _data = data;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        // If this player instance has not been initialized by calling `Init`, it has also not been registered at the
        // `PlayerManager`. This usually happens, if a player instance is placed manually and has not been created
        // using a spawner.
        // We fix this issue, by registering at the `PlayerManager` now, which will also call `Init` for us.
        if (_data == null)
        {
            var localInput = Instantiate(localInputPrefab, this.transform);
            localInput.SetColor(localControlsInitColor);

            PlayerDataKeeper.Instance.RegisterExistingInstance(this, localInput);
        }
        
        Collider = GetComponent<BoxCollider2D>();
        _tintFlashController = GetComponent<TintFlashController>();
        _renderer = GetComponent<Renderer>();
        
        _healthPoints = maxHealthPoints;
        _playerState = PlayerState.walking;
        
        var material = GetComponent<Renderer>().material;
        material.SetColor("_ShirtColor", PlayerColorMethods.ColorToRGB(this.Color));
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // Check whether the player released the item key
        if (!Input.GetButton(Button.Item) && _playerState == PlayerState.carrying && !CarriesSomeone())
            ThrowThrowable(_throwableItemInstance, new Vector2(Input.GetHorizontal(), Input.GetVertical()));
        // Check whether the player released the carry key
        else if (!Input.GetButton(Button.Carry) && _playerState == PlayerState.carrying && CarriesSomeone())
            ThrowThrowable(_otherPlayer, new Vector2(Input.GetHorizontal(), Input.GetVertical()));

        // Attacking
        if (Input.GetButtonDown(Button.Attack) && (_playerState == PlayerState.walking || _playerState == PlayerState.attacking)) {
            _playerState = PlayerState.attacking;
            Attack();
        }
        else if (_playerState == PlayerState.walking)
        {
            // Item usage
            if (Input.GetButtonDown(Button.Item) && _data.item && _data.item.Ready() && 
            _data.item.Instantiate(new Vector2(Rigidbody2D.position.x, _renderer.bounds.max.y)) is Throwable throwableItem)
                PickUpThrowable(_throwableItemInstance = throwableItem);
            // Carrying (FIXME: We should have the `Interactive` component handle this.)
            else if (Input.GetButtonDown(Button.Carry) && GetNearPlayer(out _otherPlayer)) {
                _otherPlayer.SetCarry(this);
                PickUpThrowable(_otherPlayer);
            }

        }
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
            (Vector2) transform.position + Collider.offset,  // Search from middle point of our collider
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
                Animator.SetBool(CarriedState, false);
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
            ThrowThrowable(_otherPlayer, Vector2.zero);
        // if the player is carried by another player, release 
        else if (_otherPlayer?.CarriesSomeone() ?? false) {
            _otherPlayer.ThrowThrowable(this, Vector2.zero);
            Animator.SetBool(CarriedState, false);
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
            Input.PlayVibrationFeedback(new List<float>
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

    /**
     * Displays a bar of hearts (life gauge) relative to the player avatar
     */
    private void DisplayLifeGauge()
    {
        var spriteBounds = this.GetComponent<SpriteRenderer>().bounds.size;
        // ToDo: Adjust height, so that lifeGauge and goldGauge can be displayed concurrently!
        // Set relative position of the life gauge so that it appears slightly above the player character
        this.lifeGauge.transform.localPosition =
            new Vector3(
                0,
                spriteBounds.y * 0.7f,
                0
            );
        
        this.lifeGauge.GetComponent<LifeGauge>().DrawLifeGauge(_healthPoints, maxHealthPoints);
    }

    private void DisplayCoinGauge()
    {
        // ToDo: Adjust height, so that lifeGauge and goldGauge can be displayed concurrently!
        this.coinGauge.GetComponent<CoinGaugeController>().DrawColdCounter(this._data.goldCounter);
    }

    public Vector2 GetPosition()
    {
        return Rigidbody2D.position;
    }

    private void Attack()
    {
        Animator.SetTrigger(AttackTrigger);
        fightingSounds.PlayOneShot();
    }

    /**
     * Called by attack animations, when they are finished
     */
    private void OnAttackFinished()
    {
        // After attack aninmation is done the playerstate changes back to walking
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
        _vertical = (enableVertical) ? Input.GetVertical() : 0;
        _horizontal = (enableHorizontal) ? Input.GetHorizontal() : 0;

        // If we are pulling a box and trying to move in the pulling direction, we instruct the box to pull
        if (_playerState == PlayerState.pulling && DoesMoveInPullDirection())
        {
            _pushableToPull.Pull(viewDirection.Inverse(), this);
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

    /**
     * Changes player state so that they are ready to pull an object (e.g. a Sokoban box).
     * It also stores a reference to the `Pushable` which the player can pull.
     */
    public void EnablePulling(Pushable pushable)
    {
        _playerState = PlayerState.pulling;
        Animator.SetBool(PullingState, true);
        _pushableToPull = pushable;
    }

    /**
     * Changes player state so that the player is no longer ready to pull an object (e.g. a Sokoban box).
     */
    public void DisablePulling()
    {
        _playerState = PlayerState.walking;
        Animator.SetBool(PullingState, false);
        // `Pushable` objects have a timeout before a player can push them.
        // Since a player is in constant contact with the `Pushable` during pulling, this timeout has likely run out by
        // now. Resetting the timeout when stopping to pull leads to a better experience:
        _pushableToPull?.ResetContactTimeout();
        
        _pushableToPull = null;
    }

    /**
     * Returns true if the player currently carries someone else
     */
    public bool CarriesSomeone()
    {
        return gameObject.GetComponent("HingeJoint2D") != null && _otherPlayer != null;
    }

    /**
     * When a player is being carried he needs a reference to the carrying player
     */
    public void SetCarry(PlayerController otherPlayer)
    {
        _otherPlayer = otherPlayer;
    }

    /**
     * Call this method to pickup another player
     */
    public void PickUpThrowable(Throwable throwable) 
    {
        // glue to players together
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        // Stops objects from continuing to collide and creating more joints
        joint.enableCollision = false; 
        joint.enabled = false;

        var ontop = new Vector2(Rigidbody2D.position.x, _renderer.bounds.max.y);
        StartCoroutine(throwable.PickUpCoroutine(ontop, joint, Collider));
        _playerState = PlayerState.carrying;
        Animator.SetBool(CarryingState, true);
    }

    /**
     * Call this method to throw the carried player into direction (or drop if direction is zero vector)
     */
    public void ThrowThrowable(Throwable throwable, Vector2 direction)
    {
        _playerState = PlayerState.walking;
        Animator.SetBool(CarryingState, false);

        Destroy(gameObject.GetComponent("HingeJoint2D"));
        StartCoroutine(throwable.ThrowCoroutine(direction, Collider));

        // if we throw a player, remove reference, if we throw an item it doesn't matter
        _otherPlayer = null;
    }

    /**
     * This coroutine is called on the player that is being carried
     */
    public IEnumerator PickUpCoroutine(Vector2 ontop, HingeJoint2D joint, BoxCollider2D collider)
    {
        joint.connectedBody = Rigidbody2D;
        _playerState = PlayerState.carried;

        Animator.SetBool(CarriedState, true);
        Physics2D.IgnoreCollision(collider, Collider);
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
        Physics2D.IgnoreCollision(collider, Collider, false);
        _renderer.sortingOrder--;
    }

    /**
     * A flying player should damage enemies
     */
    void OnCollisionEnter2D(Collision2D other)
    {
        if ( _playerState == PlayerState.thrown)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                var enemy = other.gameObject.GetComponent<EntityController>();
                enemy.PutDamage(1, (other.transform.position - transform.position).normalized); 
            }
            else if (other.gameObject.CompareTag("Switch"))
                other.gameObject.GetComponent<ShockSwitch>().Activate();
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


    public void IncreaseGoldCounter()
    {
        _data.goldCounter++;
        DisplayCoinGauge();
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Collectible")) {
            var param = _data.item;
            other.gameObject.GetComponent<Collectible>().Collect(ref param);
            _data.item = param;
        }
    }

    /**
     * If the player dies, they emit the OnRespawn event.
     * This function removes all subscribers from that event. Usually there is only one subscriber, which is the
     * (Re-)Spawner.
     * 
     * Hence, this method may be used to reset the respawning point.
     */
    public void ClearRespawnHandlers()
    {
        if (OnRespawn != null)
        {
            foreach (var handler in OnRespawn.GetInvocationList())
            {
                OnRespawn -= (OnRespawnAction) handler;
            }
        }
    }

    /**
     * Plays an animation in which the player presents an item.
     *
     * @param sprite   sprite of the item which is presented
     * @param callback callback which is invoked, as soon as the animation completes
     */
    public void PresentItem(Sprite sprite, PresentItemAction callback = null)
    {
        _playerState = PlayerState.presenting_item;
        _presentItemCallback = callback;
        itemPresentation.sprite = sprite;
        Animator.SetTrigger(PresentItemTrigger);
    }

    /**
     * Invoked, as whenever the item presentation animation completes.
     * It resets the player state to `walking` and invokes callbacks, if set.
     */
    public void OnPresentItemAnimationComplete()
    {
        _playerState = PlayerState.walking;
        _presentItemCallback?.Invoke();
    }

    public bool IsLookingAt(Vector3 point)
    {
        return
            Collider.bounds.DirectionTo(
                point, out var fromPlayerToPointDirection
            )
            && fromPlayerToPointDirection == viewDirection;
    }
}
