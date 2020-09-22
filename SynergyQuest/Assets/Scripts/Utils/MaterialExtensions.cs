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

public static class MaterialExtensions
{
    /**
     * Adds a new instance of the given material to the given renderer and return a reference to it.
     * You must use <see cref="MaterialExtensions.Destroy"/> to destroy the material again.
     */
    public static Material Instantiate(this Material materialPrefab, Renderer renderer)
    {
        var oldMaterials = renderer.materials;

        renderer.materials = oldMaterials.Plus(materialPrefab);
        var instance = renderer.materials[oldMaterials.Length];

        return instance;
    }

    /**
     * <summary>
     * Removes the given material from the given renderer and destroys it.
     * </summary>
     * <seealso cref="Instantiate"/>
     */
    public static void Destroy(this Material materialInstance, Renderer renderer)
    {
        renderer.materials = renderer.materials.RemoveFirst(materialInstance);
        Object.Destroy(materialInstance);
    }
}
