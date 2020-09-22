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
using System.Linq;

/**
 * Contains an assortment of extension methods of c# internal types
 */
public static class GenericExtensions
{
    /**
     * Retrieves the next value of an enum in order of their definition inside the enum.
     * 
     * (See also https://stackoverflow.com/a/1303417)
     */
    public static TEnum NextValue<TEnum>(this TEnum currentValue)
      where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
        var lowerBoundIdx = values.GetLowerBound(0);
        var upperBoundIdx = values.GetUpperBound(0);

        var idx = Array.IndexOf(values, currentValue);

        if (idx + 1 > upperBoundIdx)
        {
            return values[lowerBoundIdx];
        }

        else
        {
            return values[idx + 1];
        }
    }
}
