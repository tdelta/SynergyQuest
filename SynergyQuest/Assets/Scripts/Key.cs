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
 * A key can be collected by players and be used to open doors. See also `KeyLock`.
 *
 * Usually this class should be combined with a `ContactTrigger` which invokes the `CollectKey` method.
 */
[RequireComponent(typeof(Guid))]
public class Key : MonoBehaviour
{
    private SpriteRenderer _renderer;
    public Guid Guid { get; private set; }

    /**
     * Since this object will invisibly persist for a short while before being destroyed to play a sound,
     * we use this flag to indicate this state and prevent the key from being collected.
     *
     * This prevents multiple players from collecting a key at the same time.
     */
    private bool _isBeingDestroyed = false; 

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        Guid = GetComponent<Guid>();
        
        // Self-destruct, if this key has already been collected
        if (DungeonDataKeeper.Instance.HasKeyBeenCollected(this))
        {
            Destroy(this.gameObject);
        }
    }

    /**
     * Call this method to let a player collect this key.
     */
    public void CollectKey(PlayerController collector)
    {
        if (!_isBeingDestroyed)
        {
            // Remember, that this key has been collected
            DungeonDataKeeper.Instance.MarkKeyAsCollected(this);
            // Increase the counter of collected keys for the players
            PlayerDataKeeper.Instance.NumKeys += 1;
            // Play a sound and destroy this object afterwards
            this.gameObject.PlaySoundAndDestroy();
            _isBeingDestroyed = true;
            // Display an animation on the player collecting this key.
            collector.PresentItem(_renderer.sprite);
        }
    }
}
