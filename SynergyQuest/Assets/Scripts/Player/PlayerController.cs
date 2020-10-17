// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

using System;
using System.Collections.Generic;
using System.Linq;
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
    presenting_item,
    spring_jumping
}

[RequireComponent(typeof(Throwable))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(TintFlashController))]
[RequireComponent(typeof(ItemController))]
[RequireComponent(typeof(Spawnable))]
public class PlayerController : EntityController
{
    [SerializeField] private GameObject lifeGauge = default;
    [SerializeField] private CoinGaugeController coinGauge = default;
    
    [SerializeField] private float speed = 3.0f; // units per second
    [SerializeField] private MultiSound fightingSounds = default;
    [SerializeField] private MultiSound hitSounds = default;
    [SerializeField] private MultiSound deathSounds = default;
    [SerializeField] private MultiSound fallingSounds = default;
    [SerializeField] private InteractionSpeechBubble interactionSpeechBubble = default;
    [SerializeField] private InteractorCollider interactorCollider = default;
    [SerializeField] private int goldLossOnDeath = 10;
    [SerializeField] private CoinController coinPrefab = default;
    /**
     * Position where `Throwable`s move when being carried by this player
     */
    [SerializeField] private Transform carryPosition = default;
    public Vector2 CarryPosition => carryPosition.position;
    public Vector2 Center => Collider.bounds.center;

    public InteractionSpeechBubble InteractionSpeechBubble => interactionSpeechBubble;
    public InteractorCollider InteractorCollider => interactorCollider;

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
    [SerializeField] private LocalInput localInputPrefab = default;
    
    /**
     * There is an animation, in which a player can present an item.
     * The sprite of the item will be displayed by this renderer.
     */
    [SerializeField] private SpriteRenderer itemPresentation = default;
    
    public BoxCollider2D Collider { get; private set; }
    public SpriteRenderer spriteRenderer { get; private set; }
    private Renderer _renderer;
    private ItemController _itemController;
    private Throwable _throwable;
    public Spawnable spawnable { get; private set; }

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
    public PlayerData Data => _data;
    
    public Input Input => _data.input;

    public LinkedList<ItemDescription> CollectedItems => _data.CollectedItems;
    
    /**
     *Modeling the current action of the player
     */
    private PlayerState _playerState;

    /**
     * Reference to carried object.
     * null, if no object is currently being carried
     */
    private Throwable _carriedThrowable;
    public bool IsCarryingSomething => !ReferenceEquals(_carriedThrowable, null);
    
    /**
     * TODO: To be discussed (from Marc) : Do we need this variable as an class attribute?
     */
    private Vector2 _lookDirection = new Vector2(1,0);
    public Vector2 LookDirection => _lookDirection;
    
    /**
     * Set by the animator, makes it easier to implement logic which depends on viewDirection

     * Additional note (from Marc) : Technically we could calculate the viewDirection by the variable above (lookDirection),
     * but this is a tedious task and can be done within the animator. Maybe remove attribute above (Or keep for debug purpose?).
     */
    public Direction viewDirection;

    /**
     * If the player were to throw an item or another player, this is the direction they shall be thrown.
     */
    public Vector2 ThrowingDirection => _lookDirection.ApproximateDirection().ToVector(); // use the last movement based viewing direction and convert it into the nearest base vector

    [SerializeField] private float throwingDistance = 3.5f;
    public float ThrowingDistance => throwingDistance;
    
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
    private static readonly int SpringJumpState  = Animator.StringToHash("SpringJumping");

    public PlayerColor Color => Input.GetColor();

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

    protected override void Awake()
    {
        base.Awake();
        
        _throwable = GetComponent<Throwable>();
        Collider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _tintFlashController = GetComponent<TintFlashController>();
        _itemController = GetComponent<ItemController>();
        spawnable = GetComponent<Spawnable>();
    }

    private void OnEnable()
    {
        _throwable.OnPickedUp += OnPickedUp;
        _throwable.OnThrown += OnThrown;
        _throwable.OnLanded += OnLanded;

        spawnable.OnRespawn += OnRespawn;
    }
    
    private void OnDisable()
    {
        _throwable.OnPickedUp -= OnPickedUp;
        _throwable.OnThrown -= OnThrown;
        _throwable.OnLanded -= OnLanded;
        
        spawnable.OnRespawn -= OnRespawn;
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
        
        coinGauge.Init(this);

        _playerState = PlayerState.walking;
       
        SetShirtColor(this.Color);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        
        // Attacking
        if (Input.GetButtonDown(Button.Attack) && (_playerState == PlayerState.walking || _playerState == PlayerState.attacking)) {
            _playerState = PlayerState.attacking;
            Attack();
        }
    }
    
    private void SetShirtColor(PlayerColor color)
    {
        var material = GetComponent<Renderer>().material;
        material.SetColor("_ShirtColor", color.ToRGB());
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
            // Prevent change of velocity by collision forces
            default:
                Move(false, false);
                break;
            }
    }

    public void OnRespawn(Vector3 respawnPosition, Spawnable.RespawnReason reason)
    {
        // if the player carries another player, release
        if (CarriesSomeone())
        {
            ThrowThrowable(_carriedThrowable, Vector2.zero);
        }
        // if the player is carried by another player, release 
        else if (_throwable.IsBeingCarried) {
            _throwable.Carrier.ThrowThrowable(_throwable, Vector2.zero);
        }

        if (_data.HealthPoints <= 0)
        {
            ChangeHealth(PlayerInfo.MAX_HEALTH_POINTS);
        }
    }

    public override bool ChangeHealth(int delta, bool playSounds = true)
    {
        // if the player is thrown he shouldn't get any damage
        if (_playerState == PlayerState.thrown)
            return false;

        _data.HealthPoints += delta;

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

        if (_data.HealthPoints <= 0) {
            if (playSounds)
            {
                deathSounds.PlayOneShot();
            }

            // If we died, we want to spawn gold on our last position
            var lostGold = Math.Min(goldLossOnDeath, _data.GoldCounter);
            _data.GoldCounter -= lostGold;
            // We want to spawn the gold on the last respawn position
            // This is the last position of the player on solid ground
            var goldSpawnPosition = spawnable.RespawnPosition;
            // spawn the gold
            for (int i = 0; i < lostGold; ++i)
            {
                Instantiate(coinPrefab, goldSpawnPosition, Quaternion.identity);
            }

            spawnable.Respawn(Spawnable.RespawnReason.Death);
        }
        return true;
    }

    /**
     * Displays a bar of hearts (life gauge) relative to the player avatar
     */
    private void DisplayLifeGauge()
    {
        var spriteBounds = spriteRenderer.bounds.size;
        // ToDo: Adjust height, so that lifeGauge and goldGauge can be displayed concurrently!
        // Set relative position of the life gauge so that it appears slightly above the player character
        this.lifeGauge.transform.localPosition =
            new Vector3(
                0,
                spriteBounds.y * 0.7f,
                0
            );
        
        this.lifeGauge.GetComponent<LifeGauge>().DrawLifeGauge(_data.HealthPoints, PlayerInfo.MAX_HEALTH_POINTS);
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
            _pushableToPull.Pull(viewDirection.Inverse());
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
        if (_playerState != PlayerState.falling)
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
        return !ReferenceEquals(_carriedThrowable, null);
    }

    /**
     * Lets given player pickup this player.
     * Invoked by the interactive component.
     *
     * @param player: The player that picks us up
     */
    public void GetPickedUp(PlayerController player){
        _throwable.Pickup(player);
    }

    /**
     * Lets given player throw this player.
     * Used by the interactive component
     *
     * @param player: The player that throws us
     */
    public void GetThrown(PlayerController player){
        // have to check if we were actually being carried when carry button is released
        if (_throwable.IsBeingCarried)
            player.ThrowThrowable(_throwable, player.ThrowingDirection);
    }

    /**
     * Call this method to throw the carried object into direction (or drop if direction is zero vector)
     */
    public void ThrowThrowable(Throwable throwable, Vector2 direction)
    {
        throwable.Throw(direction);
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
            else if (other.gameObject.CompareTag("Ghost"))
                other.gameObject.GetComponent<PlayerGhost>()?.Exorcise();
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
        if (_playerState != PlayerState.falling && _playerState != PlayerState.thrown)
        {
            // While falling, the player is probably not on safe terrain, so we tell Spawnable to not safe any respawn
            // positions
            // (since we change layers during falling, Chasm may not register as unsafe terrain while falling)
            spawnable.RegisterTouchingUnsafeTerrain(this);
            
            _playerState = PlayerState.falling;
            // If the player is being moved by a platform, they should no longer be moved when falling
            PhysicsEffects.RemoveCustomOrigin();
            // We place the player at the center of its collider for the falling animation to get a position somewhat
            // centered around the point where the player entered the chasm
            PhysicsEffects.Teleport(Collider.bounds.center);
            
            Animator.SetTrigger(FallTrigger);
        }
    }
    
    /**
     * Called as soon as the fall animation started by an animation event.
     */
    public void OnFallAnimationStarted()
    {
        // A falling player will be moved on a special layer, so that it does not interact with most other objects while
        // falling.
        this.gameObject.layer = LayerMask.NameToLayer("Falling");
        
        fallingSounds.PlayOneShot();
    }

    /**
     * Called as soon as the fall animation ended by an animation event.
     */
    public void OnFallAnimationComplete()
    {
        // A falling player will be moved on a special layer, so that it does not interact with most other objects while
        // falling. We restore the original layer here.
        this.gameObject.layer = LayerMask.NameToLayer("Player");
        _playerState = PlayerState.walking;
        
        // The falling animation scales the player down.
        // We need to scale it up again. This would be done by the animator automatically, however, this will only happen
        // after this method completed, which is to late, since OnExitTrigger2D functions will then not be invoked.
        // This can lead to bugs, for example when the player was standing on a platform which needs the trigger.
        this.transform.localScale = new Vector3(1, 1, 1);
        
        // Make the player invisible until respawn
        spriteRenderer.enabled = false;

        var originalHealth = _data.HealthPoints;
        // Remove some health from player for falling
        ChangeHealth(-1, false);
        
        // Respawn them, if there health did not get down to zero
        // (which would respawn them anyway)
        if (originalHealth > 0)
        {
            spawnable.Respawn();
        }
        
        // Stop blocking Spawnable from registering respawn positions, see also comment further above
        // where the blocking started
        spawnable.UnregisterTouchingUnsafeTerrain(this);
    }


    public void IncreaseGoldCounter()
    {
        _data.GoldCounter++;
    }
    
    public bool Collect(ItemDescription itemDescription)
    {
        return _itemController.Collect(itemDescription);
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

    public bool IsLookingAt(Bounds bounds)
    {
        Collider.bounds.DirectionTo(
            bounds.center, out var dir
        );
        
        return
            Collider.bounds.DirectionTo(
                bounds.center, out var fromPlayerToPointDirection
            )
            && fromPlayerToPointDirection == viewDirection;
    }

    /**
     * Invoked by throwable behavior when player shall carry a `Throwable`.
     * It adjusts the internal state and animations.
     */
    public void InitCarryingState(Throwable carriedThrowable)
    {
        if (!IsCarryingSomething)
        {
            _carriedThrowable = carriedThrowable;
            _playerState = PlayerState.carrying;
            Animator.SetBool(CarryingState, true);
        }

        else
        {
            Debug.LogWarning("Can not carry something else while already carrying.");
        }
    }

    /**
     * Invoked by throwable behavior when player shall stop carrying a `Throwable`.
     * It adjusts the internal state and animations.
     */
    public void ExitCarryingState()
    {
        if (IsCarryingSomething)
        {
            _carriedThrowable = null;
            _playerState = PlayerState.walking;
            Animator.SetBool(CarryingState, false);
        }

        else
        {
            Debug.LogWarning("Can not exit carrying state when not carrying anything.");
        }
    }

    /**
     * Invoked by OnPickedUp event of `Throwable`, if player is being picked up by another.
     *
     * Adjusts animation, internal state and other stuff.
     */
    private void OnPickedUp(PlayerController carrier)
    {
        if (!ReferenceEquals(_carriedThrowable, null))
        {
            ThrowThrowable(_carriedThrowable, Vector2.zero);
        }
        
        Animator.SetBool(CarriedState, true);
        _playerState = PlayerState.carried;
        
        // The collisions are turned off during carrying but we dont want the interactive to react to that
        Interactive[] interactives = GetComponents<Interactive>();
        foreach(Interactive interactive in interactives) {
            if (interactive.Button == Button.Carry){
                interactive.IgnoreCollisions = true;
            }
        }
    }

    /**
     * Invoked by `OnThrow` event of `Throwable`, if player is being thrown by another.
     */
    private void OnThrown()
    {
        _playerState = PlayerState.thrown;
        
        // The collisions are turned off during carrying but we dont want the interactive to react to that
        foreach (var interactive in GetComponents<Interactive>()
            .Where(interactive => interactive.Button == Button.Carry))
        {
            interactive.IgnoreCollisions = false;
        }
    }

    /**
     * Invoked by `OnLanded` event of `Throwable`, if player lands after being thrown by another player.
     */
    private void OnLanded()
    {
        Animator.SetBool(CarriedState, false);
        _playerState = PlayerState.walking;
    }

    public void FaceDirection(Direction direction)
    {
        _lookDirection = direction.ToVector();
        _lookDirection.Normalize();
        Animator.SetFloat(LookXProperty, _lookDirection.x);
        Animator.SetFloat(LookYProperty, _lookDirection.y);
    }
    
    /**
     * <summary>
     * Adjusts the player sprite so that it visually fits the jump animation performed by <see cref="Spring"/>.
     * Also adjusts the internal state to indicate, that the player is currently controlled by the <see cref="Spring"/>
     * behavior and should not move by itself, etc.
     * </summary>
     * <param name="onOff">
     *     The state will be adjusted as stated above if true.
     *     Otherwise, the sprite adjustments will be reverted and the player state will return to
     *     <see cref="PlayerState.walking"/>
     * </param>
     */
    public void SetSpringJumpMode(bool onOff)
    {
        _playerState = onOff ? PlayerState.spring_jumping : PlayerState.walking;
        Animator.SetBool(SpringJumpState, onOff);
    }
}
