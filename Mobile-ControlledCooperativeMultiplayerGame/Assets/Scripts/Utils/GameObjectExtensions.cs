using Cinemachine;
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

    /**
     * <summary>
     * Can set all <see cref="CinemachineTargetGroup"/>s to either follow or not follow an object.
     * </summary>
     */
    public static void SetFollowedByCamera(this GameObject self, bool followed)
    {
        if (followed)
        {
            foreach (var cameraTargetGroup in Object.FindObjectsOfType<CinemachineTargetGroup>())
            {
                var radius = 0.0f;
                if (self.GetComponent<Collider2D>() is Collider2D collider && collider != null)
                {
                    var bounds = collider.bounds;
                    radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
                }
                
                cameraTargetGroup.AddMember(
                    self.transform,
                    1,
                    radius
                );
            }
        }

        else
        {
            foreach (var cameraTargetGroup in Object.FindObjectsOfType<CinemachineTargetGroup>())
            {
                cameraTargetGroup.RemoveMember(self.transform);
            }
        }
    }
}
