using System.Runtime.InteropServices;
using UnityEngine;

/**
 * Simulates certain physics effects (knockback) without Unity's `AddForce`.
 * 
 * # Reasoning
 * 
 * Most of the time we want Zelda-like motion. This means we can not use the ApplyForce methods of a rigidbody to move
 * an entity, since this results in a sliding motion.
 * We can also not completely get rid of rigidbodies because we need them for handling collisions.
 * Thus we move entities in general by the "MovePosition" method of rigidbodies manually.
 *
 * This has the downside, that we can no longer apply forces to it in the few cases we actually want to, since
 * MovePosition cancels the forces out. One case where we want to apply a force is knockback on damage.
 *
 * This class simulates the application of forces / impulses by manipulating positions directly.
 * Its kind of hacky, but it seems to work.
 */
public class PhysicsEffects
{
    // Should be 9.81 but 10 will do for us
    private const float GravitationalAcceleration = 10.0f; // m/(s^2)
    // We do not perform speed manipulations below this threshold to avoid very slow sliding
    private const float MinSpeed = 0.1f;

    // kinetic / sliding coefficient of friction
    // (https://en.wikipedia.org/wiki/Friction#Coefficient_of_friction)
    private const float frictionCoefficient = 0.7f;

    private Rigidbody2D _rigidbody2D;
    
    private Vector2 _currentSpeed = Vector2.zero;

    public PhysicsEffects(Rigidbody2D rigidbody)
    {
        this._rigidbody2D = rigidbody;
    }
    
    private Vector2 ComputeFrictionDeceleration()
    {
        var mass = _rigidbody2D.mass;
        
        var normalForceMagnitude = mass * GravitationalAcceleration;
        var frictionForceMagnitude = frictionCoefficient * normalForceMagnitude;
        var deceleration = frictionForceMagnitude / mass;
        
        return -_currentSpeed.normalized * deceleration;
    }

    public void ApplyImpulse(Vector2 impulse)
    {
        _currentSpeed += impulse / _rigidbody2D.mass;
    }

    public void MoveBody(Vector2 nextMovementPosition)
    {
        var frictionDeceleration = ComputeFrictionDeceleration();

        // Actually, we would need to ensure, that there is no sign change due to friction and clamp at 0, however,
        // Together with the minimum speed logic this seems to work for now, as long as deceleration does not get too 
        // high.
        _currentSpeed += frictionDeceleration * Time.deltaTime;
        
        // Avoid slow sliding by defining a minimum effect speed
        if (Mathf.Abs(_currentSpeed.magnitude) < MinSpeed)
        {
            _currentSpeed = Vector2.zero;
        }

        nextMovementPosition += _currentSpeed * Time.deltaTime;

        _rigidbody2D.MovePosition(nextMovementPosition);
    }
}
