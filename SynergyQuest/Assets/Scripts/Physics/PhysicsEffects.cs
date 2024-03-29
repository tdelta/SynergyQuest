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

using System.Collections.Generic;
using UnityEngine;

/**
 * Simulates certain physics effects (e.g. knockback) without Unity's `AddForce`.
 *
 * Supported effects:
 *  1. Applying impulses (knockback)
 *  2. Setting a custom origin which will be followed
 *
 * Usage:
 *     1. Add this behavior to your game object
 *     2. Apply effects (e.g. `ApplyImpulse` method) whenever appropriate
 *     3. **ALWAYS** use the `MoveBody` method of this class for movement, not `rigidbody.MovePosition`
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
[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsEffects: MonoBehaviour
{
    public const float AirResistance = 15.0f; // Force, units/(s^2)
    // Used for throwing stuff. We use slightly higher gravity than usual
    public const float GravitationalAcceleration = 20.0f; // units/(s^2)
    // Should be 9.81 but 10 will do for us
    private const float RealWorldGravitationalAcceleration = 10.0f; // m/(s^2)
    // We do not perform speed manipulations below this threshold to avoid very slow sliding
    private const float MinSpeed = 0.1f;
    
    // FIXME: We should put the above constants into a scriptable object

    // kinetic / sliding coefficient of friction
    // (https://en.wikipedia.org/wiki/Friction#Coefficient_of_friction)
    private const float frictionCoefficient = 0.7f;
    public bool FrictionEnabled = true;
    
    #pragma warning disable 0109
    // Rigidbody of the object which we are applying effects to
    public new Rigidbody2D rigidbody2D { get; private set; }
    #pragma warning restore 0109

    /**
     * One may set a transform as custom origin.
     * If set, the object affected by this PhysicsEffects instance moves relative to the transform.
     * Hence, it is also moved, if the origin moves.
     */
    private Transform _customOrigin = null;
    public Transform CustomOrigin => _customOrigin;
    // We cache the last position of the custom origin since the last update here, so that we can use it to determine
    // whether the origin moved since the last frame and by how much.
    private Vector2 _lastCustomOriginPosition;
    // Whenever the origin moved, we also need to adjust the position the physics are currently interpolating to, hence,
    // we cache that here too.
    private Vector2 _lastNextPosition;
    
    /**
     * The effects applied by this class result in additional speed changes.
     * These speed changes are recorded here.
     * 
     * Note that this an additional speed for changes on top of what is already applied by an entity controller.
     * Hence, it can for example be 0 while the entity is moving.
     */
    private Vector2 _effectsSpeed = Vector2.zero;
    /**
     * Forces being continuously applied
     */
    private List<ForceEffect> _effectsForces = new List<ForceEffect>();

    /**
     * <summary>
     * Stores the last direction the player intentionally moved into. Normalized or zero.
     * (It does not report directions the player has been pushed to by other objects.)
     * </summary>
     */
    public Vector2 SteeringDirection { get; private set; } = Vector2.zero;
    
    private Vector2 _lastPosition = Vector2.zero;
    public Vector2 MovementDelta { get; private set; } = Vector2.zero;
    
    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        if (_customOrigin != null)
        {
            _lastCustomOriginPosition = _customOrigin.position;
        }

        _lastPosition = rigidbody2D.position;
    }

    /**
     * Computes the deceleration generated by kinetic friction.
     * (https://en.wikipedia.org/wiki/Friction#Kinetic_friction)
     * 
     * It only applies to velocities introduced by impulse effects and does not influence the players movement otherwise.
     */
    private Vector2 ComputeFrictionDeceleration()
    {
        var mass = rigidbody2D.mass;
        
        var normalForceMagnitude = mass * RealWorldGravitationalAcceleration;
        var frictionForceMagnitude = frictionCoefficient * normalForceMagnitude;
        var deceleration = frictionForceMagnitude / mass;
        
        return -_effectsSpeed.normalized * deceleration;
    }

    /**
     * Instantly transports player to new position
     */
    public void Teleport(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        _lastPosition = position;
        MovementDelta = Vector2.zero;
        rigidbody2D.position = position;
    }

    /**
     * Applies speed changes to an entities movement according to the given impulse.
     */
    public void ApplyImpulse(Vector2 impulse)
    {
        _effectsSpeed += impulse / rigidbody2D.mass;
    }

    /**
     * Applies a force which will be continuosly applied to this object.
     * 
     * In contrast to `AddForce` of Rigidbody2D, this class keeps track of the individual forces being applied and
     * returns a handle to the force effect.
     * This handle can be used to remove the force later.
     */
    public ForceEffect ApplyForce(Vector2 force)
    {
        var effect = new ForceEffect(force);
        _effectsForces.Add(effect);

        return effect;
    }

    /**
     * Remove a force.
     * This also removes any velocity already induced by the force.
     */
    public void RemoveForce(ForceEffect forceEffect)
    {
        _effectsForces.Remove(forceEffect);
    }
    
    /**
     * Sets a transform as coordinate origin for this object.
     * 
     * If set, the object affected by this PhysicsEffects instance moves relative to the transform.
     * Hence, it is also moved, if the origin moves.
     */
    public void SetCustomOrigin(Transform customOrigin)
    {
        _customOrigin = customOrigin;

        // We cache the last position of the custom origin since the last update here, so that we can use it to determine
        // whether the origin moved since the last frame and by how much.
        _lastCustomOriginPosition = customOrigin.position;
    }

    /**
     * Removes a custom origin set by `SetCustomOrigin`.
     * 
     * The object affected by this `PhysicsEffects` instance retains its world space coordinates but will no longer
     * follow the custom origin.
     */
    public void RemoveCustomOrigin()
    {
        _customOrigin = null;
    }
    
    /**
     * Applies effects to movements if there are any. If there are no effects, this is the same as calling
     * rigidbody.MovePosition, hence, **ALWAYS** use this method instead of `rigidbody.MovePosition` if you want
     * effects to apply.
     *
     * @param nextMovementPosition next position to move to. It will be appropriately altered according to current
     *                             effects. Since `rigidbody.MovePosition` is used internally, interpolation settings
     *                             of the rigidbody apply.
     */
    public void MoveBody(Vector2 nextMovementPosition)
    {
        // Apply friction
        //
        // Actually, we would need to ensure, that there is no sign change due to friction and clamp at 0, however,
        // together with the minimum speed logic this seems to work for now, as long as deceleration does not get too 
        // high.
        if (FrictionEnabled)
        {
            var frictionDeceleration = ComputeFrictionDeceleration();
            _effectsSpeed += frictionDeceleration * Time.deltaTime;
        }
        
        // Avoid slow sliding by defining a minimum effect speed
        if (Mathf.Abs(_effectsSpeed.magnitude) < MinSpeed)
        {
            _effectsSpeed = Vector2.zero;
        }

        var speed = _effectsSpeed;
        // Compute effect of forces and apply speed induced by them:
        if (!Mathf.Approximately(rigidbody2D.mass, 0))
        {
            foreach (var forceEffect in _effectsForces)
            {
                forceEffect.EffectSpeed += forceEffect.Force * Time.deltaTime;
                speed += forceEffect.EffectSpeed;
            }
        }

        nextMovementPosition += speed * Time.deltaTime;

        rigidbody2D.MovePosition(nextMovementPosition);
        // Cache the position the rigidbody is moving towards. We need it when a custom origin has been set, see the
        // `Update` method.
        _lastNextPosition = nextMovementPosition;
        
        // Remember the direction the player wants to move (if the difference between the current and the target position is big enough)
        var steeringDirection = _lastNextPosition - rigidbody2D.position;
        SteeringDirection = steeringDirection.magnitude > 0.01 ? steeringDirection.normalized : Vector2.zero;
    }
    
    private void Update()
    {
        // Check whether a custom origin has been set which we must follow.
        if (!ReferenceEquals(_customOrigin, null))
        {
            var originPosition = (Vector2) _customOrigin.position;
            // Determine by how much the origin moved since last frame
            var originDelta = originPosition - _lastCustomOriginPosition;

            // We must move the physics body by that amount (hard teleport).
            // We do this in `Update` and not in `FixedUpdate` since the origin can change every frame, not only every
            // physics frame.
            rigidbody2D.position += originDelta;
            
            // _lastNextPosition caches the position the rigidbody is currently moving towards.
            // Hence we also need to adjust it and inform the rigidbody:
            _lastNextPosition += originDelta;
            rigidbody2D.MovePosition(_lastNextPosition);
            
            // Cache the new position of the origin
            _lastCustomOriginPosition = originPosition;
        }
    }

    private void FixedUpdate()
    {
        var currentPosition = rigidbody2D.position;
        
        MovementDelta = currentPosition - _lastPosition;
        _lastPosition = currentPosition;
    }

    public Vector2 GetImpulse()
    {
        return _effectsSpeed;
    }
}
