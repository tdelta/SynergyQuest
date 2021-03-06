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

using System.IO;
using System.Linq;

public static class StringExtensions
{
    /**
     * <summary>
     * Replaces any backslashes (<c>\</c>) with slashes (<c>/</c>) in a string.
     * </summary>
     * <remarks>
     * Unity only accepts forward slashes for Asset paths, hence we use this method to eliminate "\" on windows systems.
     * </remarks>
     */
    public static string WinToNixPath(this string self)
    {
        return self.Replace('\\', '/');
    }
    
    /**
     * <summary>
     * Replaces any slashes (<c>/</c>) with backslashes (<c>\</c>) in a string.
     * </summary>
     */
    public static string NixToWinPath(this string self)
    {
        return self.Replace('/', '\\');
    }

    /**
     * <summary>
     * Removes any (back-)slashes (<c>/</c> or <c>\</c>) and replaces them with <see cref="Path.DirectorySeparatorChar"/>.
     * </summary>
     */
    public static string CorrectFsSlashes(this string self)
    {
        return self
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
    }

    /**
     * <summary>
     * Removes the first n lines from a string.
     * </summary>
     */
    public static string RemoveLeadingLines(this string self, int lines)
    {
        return string.Join(
            "\n",
            self
                .Split('\n')
                .Skip(lines)
        );
    }
}
