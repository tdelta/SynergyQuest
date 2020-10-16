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
 * ScriptableObject class which holds an array of absorbable gameObjects. This is used for e.g. monster drops to make
 * it possible to add new absorbable gameObjects to all Monsters at the same time.
 */
[CreateAssetMenu(fileName = "Absorbables", menuName = "ScriptableObjects/Absorbables")]
public class Absorbables : ScriptableObject
{
    [SerializeField] private AbsorbableController[] absorbables = default;

    public AbsorbableController this[int index] => absorbables[index];
    public int Length => absorbables.Length;

}


