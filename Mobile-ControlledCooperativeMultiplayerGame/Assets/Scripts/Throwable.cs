using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(Collider2D), typeof(PhysicsEffects))]
public class Throwable : MonoBehaviour
{
    private Renderer _renderer;
    private Collider2D _collider;
    private PhysicsEffects _physicsEffects;
    private HingeJoint2D _hingeJoint2D;

    public delegate void PickedUpAction(PlayerController carrier);
    public event PickedUpAction OnPickedUp;
    
    public delegate void ThrownAction();
    public event ThrownAction OnThrown;
    
    public delegate void LandedAction();
    public event LandedAction OnLanded;

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

    public void Pickup(PlayerController carrier)
    {
        _carrier = carrier;
        IsBeingCarried = true;
        StartCoroutine(PickUpCoroutine());
    }

    public void Throw(Vector2 direction)
    {
        StartCoroutine(ThrowCoroutine(direction));
    }
    
    private IEnumerator PickUpCoroutine()
    {
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
        _physicsEffects.ApplyImpulse(10 * direction);
        _hingeJoint2D.connectedBody = null;
        _hingeJoint2D.enabled = false;

        IsBeingCarried = false;
        OnThrown?.Invoke();

        // restore sorting order & collision between the two players, when player leaves state thrown
        yield return new WaitUntil(() => _physicsEffects.GetImpulse() == Vector2.zero);
        Physics2D.IgnoreCollision(_collider, _carrier.Collider, false);
        _renderer.sortingOrder--;
        
        OnLanded?.Invoke();
    }
}
