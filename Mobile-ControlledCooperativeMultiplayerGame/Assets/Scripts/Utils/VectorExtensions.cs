using UnityEngine;

/**
 * Extension functions for vectors
 */
public static class VectorExtensions
{
    /**
     * Whether two vectors are pointing in the same general direction (the angle between them is less than 90°).
     */
    public static bool IsSameDirectionAs(this Vector2 self, Vector2 other)
    {
        return Vector2.Dot(self, other) > 0;
    }
}
