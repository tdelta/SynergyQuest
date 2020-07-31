using UnityEngine;

public static class GameObjectExtensions
{
    // FIXME: Implement as proper animation
    public static void Shrink(this GameObject self, Vector3 scaleFactor)
    {
        if (self.GetComponent<PhysicsEffects>() is PhysicsEffects effects)
        {
            if (effects.GetImpulse() == Vector2.zero)
            {
                self.transform.localScale -= scaleFactor;
            }
        }
    }
}
