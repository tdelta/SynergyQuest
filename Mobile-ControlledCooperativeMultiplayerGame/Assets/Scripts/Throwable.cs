using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

/**
 * Behavior which allows an object to be picked up and thrown by players.
 */
[RequireComponent(typeof(Renderer), typeof(Collider2D), typeof(PhysicsEffects))]
public class Throwable : MonoBehaviour
{
    private Renderer _renderer;
    private Collider2D _collider;
    private PhysicsEffects _physicsEffects;
    private HingeJoint2D _hingeJoint2D;

    /**
     * When this object hits the ground, we let it slightly slide further with this velocity
     * FIXME: Put this in a scriptable object.
     */
    private const float SlideSpeed = 2.0f;

    /**
     * When being carried, we move our rigidbody to a special layer "Carried", so that it does not interact with most
     * objects (Chasms, Platforms).
     *
     * In this field we temporarily store our original layer, so that we can restore it, as soon as we land again
     * (after throwing).
     */
    private int _cachedLayer;
    /**
     * Turns out, the movement of the carrying player can be dragged down by the mass of this object when carrying.
     * Hence, we set our mass to 0 while being carried.
     * In this field we temporarily store the original mass, so that we can restore it, as soon as this object is being
     * thrown.
     */
    private float _cachedMass;

    /**
     * Event which is invoked if this object has just been picked up by a player
     */
    public event PickedUpAction OnPickedUp;
    public delegate void PickedUpAction(PlayerController carrier);
    
    /**
     * Event which is invoked if this object has just been thrown by a player
     */
    public event ThrownAction OnThrown;
    public delegate void ThrownAction();
    
    /**
     * Event which is invoked if this object has just landed after being thrown by a player.
     */
    public event LandedAction OnLanded;
    public delegate void LandedAction();

    /**
     * Player which is currently carrying this object.
     * It is set to null, if the object is not currently being carried
     */
    private PlayerController _carrier;
    public PlayerController Carrier => _carrier;

    public bool IsBeingCarried { get; private set; } = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
        _hingeJoint2D = this.gameObject.AddComponent<HingeJoint2D>();

        _hingeJoint2D.enabled = false;
    }

    private void Update()
    {
        if (IsBeingCarried && Carrier.Input.GetButtonDown(Button.Throw))
        {
            _carrier.ThrowThrowable(this, _carrier.ThrowingDirection);
        }
    }

    /**
     * Let the given player pick up this object.
     */
    public void Pickup(PlayerController carrier)
    {
        if (!IsBeingCarried)
        {
            // A player who is already carrying something can not pickup something else
            if (!carrier.IsCarryingSomething)
            {
                _carrier = carrier;
                IsBeingCarried = true;
                
                foreach (var interactive in GetComponents<Interactive>())
                {
                    interactive.enabled = false;
                }
                
                // Cache original layer and change layer to "Carried", see description of _cachedLayer field for explanation
                _cachedLayer = this.gameObject.layer;
                this.gameObject.layer = LayerMask.NameToLayer("Carried");
                
                // Cache original mass and change mass to 0, see description of _cachedMass field for explanation
                _cachedMass = _physicsEffects.rigidbody2D.mass;
                _physicsEffects.rigidbody2D.mass = 0;
                
                _hingeJoint2D.connectedBody = _carrier.Rigidbody2D;
                Physics2D.IgnoreCollision(_collider, _carrier.Collider);
                // temporally change sorting order to draw carried gameobject on top
                _renderer.sortingOrder++;
                _physicsEffects.Teleport(_carrier.CarryPosition);
                
                _carrier.InitCarryingState(this);
                _carrier.Input.EnableButtons((Button.Throw, true));
                OnPickedUp?.Invoke(_carrier);
                
                StartCoroutine(PickUpCoroutine());
            }
        }

        else
        {
            Debug.LogWarning("Tried to pick up object, which is already being carried.");
        }
    }

    /**
     * If this object is currently being carried by a player,
     * throw it in the given direction.
     *
     * @param direction direction into which this object shall be thrown.
     *                  You can use `Vector2.zero` to just drop it.
     *                  You can use `Carrier.ThrowDirection` to throw it in the
     *                  direction the carrying player is currently walking into.
     */
    public void Throw(Vector2 direction)
    {
        if (IsBeingCarried)
        {
            this.gameObject.layer = LayerMask.NameToLayer("Thrown");
            _physicsEffects.rigidbody2D.mass = _cachedMass;
            _physicsEffects.FrictionEnabled = false;
            
            ApplyThrowingMovement(direction);
            
            _hingeJoint2D.connectedBody = null;
            _hingeJoint2D.enabled = false;

            IsBeingCarried = false;
            _carrier.ExitCarryingState();
            _carrier.Input.EnableButtons((Button.Throw, false));
            OnThrown?.Invoke();
        }

        else
        {
            Debug.LogError("Tried to throw object which is not being carried!");
        }
    }
    
    private IEnumerator PickUpCoroutine()
    {
        // the joint should be disabled until the carried player moved ontop of the carrying player,
        // because a joint disallows such movements
        yield return new WaitForFixedUpdate(); 
        _hingeJoint2D.enabled = true;
    }
    
    /**
     * Performs all computations and applies impulses and forces required to simulate a throw
     */
    private void ApplyThrowingMovement(Vector2 directionVec)
    {
        var direction = directionVec.ApproximateDirection();
        
        // We use the center of the colliders for the physics based movement performed here
        var position = _collider.bounds.center;
        var carrierPosition = _carrier.Collider.bounds.center;
        
        // Compute throwing distance. If we are throwing down, we add the additional distance to the carriers center
        // (+ ~distance from head to feet of carrier)
        var distance = _carrier.ThrowingDistance;
        if (direction is Direction.Down)
        {
            distance += position.y - carrierPosition.y;
        }
        
        // Conversely, we remove this distance when throwing upwards
        else if (direction is Direction.Up)
        {
            distance -= position.y - carrierPosition.y;
        }
        
        var gravity = -PhysicsEffects.GravitationalAcceleration;
        var airResistance = -PhysicsEffects.AirResistance;
        
        // If thrown horizontally, we should fall down this distance onto the same Y-level the carrier was standing on
        var fallDistance = position.y - carrierPosition.y;
        
        // Compute the time it takes for us to fall down.
        // Based on the equation
        //   d = vt +0.5at^2 => t = sqrt(2d/a) when v = 0
        // Where d is the travelled distance, v is the initial velocity, t is time and a is acceleration
        var fallAccel = gravity / _cachedMass;
        var fallTime = Mathf.Sqrt(Mathf.Abs(2 * fallDistance / fallAccel));
        
        // The time it takes to complete the throw should be the same as the time it takes to reach the ground
        var throwTime = fallTime;
        // based on this time and the above equation, we can compute the initial velocity this object needs to be
        // thrown with, to travel the intended distance before reaching the ground
        var throwingSpeed = (distance - airResistance / 2 * throwTime * throwTime) / throwTime;

        // From this required speed we can compute the required impulse and apply it
        var throwImpulse = throwingSpeed * _cachedMass * directionVec;
        _physicsEffects.ApplyImpulse(throwImpulse);
        
        // Apply the force created by air resistance
        var airResistanceEffect = _physicsEffects.ApplyForce(airResistance * directionVec);

        // When hitting the ground, we will slightly slide on with this impulse
        var slidingImpulse = directionVec * (SlideSpeed * _cachedMass);
        
        // If we are thrown horizontally, apply the force of gravity
        if (direction is Direction.Left || direction is Direction.Right)
        {
            var gravityEffect = _physicsEffects.ApplyForce(gravity * Vector2.up);
            
            // When the fall completes, we want to
            // * remove the applied forces
            // * remove most of the applied impulse (to simulate strong friction / a stopping effect when hitting the ground)
            StartCoroutine(
                FallStopper(
                    fallTime,
                    -throwImpulse + slidingImpulse,
                    airResistanceEffect,
                    gravityEffect
                )
            );
        }

        else
        {
            // Same as above if-branch but without gravity
            StartCoroutine(
                FallStopper(fallTime,
                    -throwImpulse + slidingImpulse,
                    airResistanceEffect,
                    null
                )
            );
        }
    }
    
    /**
     * This coroutine waits for the moment where this object is supposed to connect to the ground after being thrown.
     */
    private IEnumerator FallStopper(float stopTime, Vector2 antiThrowImpulse, ForceEffect airResistanceEffect, [CanBeNull] ForceEffect gravityEffect)
    {
        yield return new WaitForSeconds(stopTime);
        
        // Remove all forces which simulated the flow
        _physicsEffects.RemoveForce(airResistanceEffect);
        if (gravityEffect != null)
        {
            _physicsEffects.RemoveForce(gravityEffect);
        }
        // Cancel out the impulse which initiated the throw and prevent this object from sliding forever by reenabling
        // friction
        _physicsEffects.ApplyImpulse(antiThrowImpulse);
        _physicsEffects.FrictionEnabled = true;
        
        // restore sorting order & collision between the carrier and this object, as this object lands on the ground
        this.gameObject.layer = _cachedLayer;
        Physics2D.IgnoreCollision(_collider, _carrier.Collider, false);
        _renderer.sortingOrder--;
        
        foreach (var interactive in GetComponents<Interactive>())
        {
            interactive.enabled = true;
        }
        
        OnLanded?.Invoke();
    }

    private void OnDisable()
    {
        // If we are disabled (for example if this is a bomb which just exploded), we also stop being carried
        if (IsBeingCarried)
        {
            Throw(Vector2.zero);
        }
    }
}
