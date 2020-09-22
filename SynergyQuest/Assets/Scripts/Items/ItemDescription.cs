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
 * Stores describing data about an item:
 *
 * * item prefab
 * * button to activate the item
 * * cooldown until item can be used again
 */
[CreateAssetMenu(fileName = "ItemDescription", menuName = "Items/ItemDescription", order = 0)]
public class ItemDescription : ScriptableObject
{
    /**
     * Prefab which can be used to instantiate the described item
     */
    [SerializeField] private Item itemInstancePrefab = default;
    public Item ItemInstancePrefab => itemInstancePrefab;

    /**
     * Which button must a player press to activate the item?
     */
    [SerializeField] private Button useButton = default;
    public Button UseButton => useButton;

    /**
     * How long must the player wait after activating an item, before it can be activated again?
     * (in seconds)
     */
    [SerializeField] private float cooldown = 0.0f;
    public float Cooldown => cooldown;
}
