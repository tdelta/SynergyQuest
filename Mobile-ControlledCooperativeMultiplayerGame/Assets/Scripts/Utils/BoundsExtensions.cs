using UnityEngine;

/**
 * Contains extension methods for the Unity Bounds class
 */
public static class BoundsExtensions
{
    /**
     * Computes the length of the union of the intervals [aMin; aMax] and [bMin; bMax].
     *
     * It is used by `GetAxisOverlap` to determine how much the projections of Bounds rectangles on the coordinate axes
     * overlap.
     */
    private static float ComputeAxisOverlap(float aMin, float aMax, float bMin, float bMax)
    {
        // Are the intervals completely separate?
        if (bMin > aMax || aMin > bMax)
        {
            return 0;
        }

        // Otherwise, compute their overlap
        else
        {
            var overlapMin = Mathf.Max(aMin, bMin);
            var overlapMax = Mathf.Min(aMax, bMax);
            
            return overlapMax - overlapMin;
        }
    }
    
    
    /**
     * Computes how much the given bounds overlap regarding the different coordinate axes.
     *
     * # 2D example
     * Here are two rectangles A and B. The "#" signs show their overlap on the x and y axes respectively:
     * 
     * :
     * :  +------+
     * :  |  A   |
     * #  |   +--|-----+
     * #  |   |  |  B  |
     * #  +---|--|     |
     * :      +--------+
     * :
     * .......####.......
     *
     * So there is an overlap of 4 units in the x dimension and 3 units in the y dimension.
     */
    public static Vector3 GetAxesOverlap(this Bounds lhs, Bounds rhs)
    {
        var lhsMin = lhs.min;
        var lhsMax = lhs.max;

        var rhsMin = rhs.min;
        var rhsMax = rhs.max;

        return new Vector3(
            ComputeAxisOverlap(lhsMin.x, lhsMax.x, rhsMin.x, rhsMax.x),
            ComputeAxisOverlap(lhsMin.y, lhsMax.y, rhsMin.y, rhsMax.y),
            ComputeAxisOverlap(lhsMin.z, lhsMax.z, rhsMin.z, rhsMax.z)
        );
    }
}
