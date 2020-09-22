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
