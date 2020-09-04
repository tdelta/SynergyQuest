using System;
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

    /**
     * <summary>
     * Returns a copy of <see cref="lhs"/>, but the x and y components of the vector are replaced with the values of
     * <see cref="rhs"/>.
     * </summary>
     */
    public static Vector3 Assign2D(Vector3 lhs, Vector2 rhs)
    {
        return new Vector3(
            rhs.x,
            rhs.y,
            lhs.z
        );
    }
}
