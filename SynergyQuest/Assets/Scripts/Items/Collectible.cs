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

﻿using UnityEngine;

/**
 * An object which can be collected by players and will then give them an item.
 */
[RequireComponent(typeof(ContactTrigger))]
public class Collectible : MonoBehaviour
{
    /**
     * Description of the item which can be obtained when collecting this collectible
     */
    [SerializeField] private ItemDescription itemDescription = default;

    /**
     * Gives the item of this collectible to a player.
     * It is invoked by the `ContactTrigger` component, as soon as a player touches this object.
     *
     * If it was collected, the collectible will be destroyed.
     */
    public void Collect(PlayerController collector)
    {
        if (collector.Collect(itemDescription))
            Destroy(gameObject);
    }
}
