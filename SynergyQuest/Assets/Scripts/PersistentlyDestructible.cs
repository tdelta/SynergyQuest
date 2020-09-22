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
 * Ensures an object is not spawning again when reloading a scene, if it has been destroyed manually before.
 * ("Manually" means, it has not been destroyed by unloading the scene)
 *
 * This is for example important for destructible walls which should still be destroyed if a player revisits a room.
 */
[RequireComponent(typeof(Guid))]
public class PersistentlyDestructible : MonoBehaviour
{
    public Guid guid { get; private set; }
    private bool _applicationHasQuit = false;

    private void Awake()
    {
        guid = GetComponent<Guid>();

        // If this object has been destroyed previously, prevent respawning by self-destructing on initialization
        if (DungeonDataKeeper.Instance.HasObjectBeenDestroyed(this))
        {
            Destroy(this.gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        _applicationHasQuit = true;
    }

    private void OnDestroy()
    {
        // Mark object as destroyed, but only if it is not being destroyed due to loading another scene (or closing the application)
        if (!_applicationHasQuit && !SceneController.Instance.IsLoadingScene)
        {
            DungeonDataKeeper.Instance.MarkAsDestroyed(this);
        }
    }
}
