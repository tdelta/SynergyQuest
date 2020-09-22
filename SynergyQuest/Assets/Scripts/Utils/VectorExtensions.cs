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

    /**
     * <summary>
     * Returns component with maximum value (i.e. x or y).
     * </summary>
     */
    public static float MaxComponent(this Vector2 self)
    {
        return Mathf.Max(self.x, self.y);
    }
}
