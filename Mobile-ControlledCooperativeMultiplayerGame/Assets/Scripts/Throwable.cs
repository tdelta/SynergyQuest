using System.Collections;
using UnityEditor.EventSystems;
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

    /**
     * Let the given player pick up this object.
     */
    public void Pickup(PlayerController carrier)
    {
        if (!IsBeingCarried)
        {
            _carrier = carrier;
            IsBeingCarried = true;
            StartCoroutine(PickUpCoroutine());
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
            StartCoroutine(ThrowCoroutine(direction));
        }

        else
        {
            Debug.LogError("Tried to throw object which is not being carried!");
        }
    }
    
    private IEnumerator PickUpCoroutine()
    {
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
        OnPickedUp?.Invoke(_carrier);

        // the joint should be disabled until the carried player moved ontop of the carrying player,
        // because a joint disallows such movements
        yield return new WaitForFixedUpdate(); 
        _hingeJoint2D.enabled = true;
    }
    
    /**
     * This coroutine is called when a carried player is dropped or thrown
     */
    private IEnumerator ThrowCoroutine(Vector2 direction)
    {
        _physicsEffects.rigidbody2D.mass = _cachedMass;
        _physicsEffects.ApplyImpulse(10 * direction);
        _hingeJoint2D.connectedBody = null;
        _hingeJoint2D.enabled = false;

        IsBeingCarried = false;
        OnThrown?.Invoke();

        // restore sorting order & collision between the two players, when player leaves state thrown
        yield return new WaitUntil(() => _physicsEffects.GetImpulse() == Vector2.zero);
        this.gameObject.layer = _cachedLayer;
        Physics2D.IgnoreCollision(_collider, _carrier.Collider, false);
        _renderer.sortingOrder--;
        
        OnLanded?.Invoke();
    }
}
