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
 * Base class for all items.
 * 
 * You should also create an `ItemDescription` instance, and assign your item prefab to it.
 * Then you can assign the description to `Collectible` objects in scenes.
 */
abstract public class Item : MonoBehaviour
{
    protected ItemDescription ItemDescription { get; private set; }
    
    /**
     * Called by `ItemController` when the player starts using this item after instantiating it.
     * Override `OnActivate` on sub classes to react to this event.
     *
     * @param player          the player who startet to use this item
     * @param itemDescription data related to item (prefab, button, ...)
     */
    public void Activate(PlayerController player, ItemDescription itemDescription)
    {
        ItemDescription = itemDescription;
        OnActivate(player);
    }
    
    /**
     * Called when the player starts using this item after instantiating it.
     * Should be overriden by sub classes.
     *
     * @param player          the player who startet to use this item
     */
    protected abstract void OnActivate(PlayerController player);
}
