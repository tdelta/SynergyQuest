using UnityEngine;

/**
 * Keeps track of a force applied to a `PhysicsEffects` instance.
 */
public class ForceEffect
{
    // Force being applied
    public Vector2 Force;
    // Velocity changes induced so far by the force
    public Vector2 EffectSpeed = Vector2.zero;

    public ForceEffect(Vector2 force)
    {
        Force = force;
    }
}
