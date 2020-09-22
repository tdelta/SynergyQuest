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

using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * This component marks a game object to be able to be transported by a platform, see also <see cref="Platform"/>.
 * 
 * It also keeps track of all platforms the object currently touches so that this information can be used by the
 * <see cref="Chasm"/> class to determine whether a player can not fall down a chasm because they are standing on a
 * platform.
 * </summary>
 */
public class PlatformTransportable : MonoBehaviour
{
    private Spawnable _spawnable;
    private Collider2D _collider;
    
    /**
     * Platforms this game object is currently in contact with
     */
    public HashSet<Platform> PlatformsInContact { get; } = new HashSet<Platform>();

    private void Awake()
    {
        _spawnable = GetComponent<Spawnable>();
        _collider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // If this object has a <see cref="Spawnable"/>, we register to its <c>OnRespawn</c> event.
        if (!ReferenceEquals(_spawnable, null))
        {
            _spawnable.OnRespawn += OnRespawn;
        }
    }
    
    private void OnDisable()
    {
        // If this object has a <see cref="Spawnable"/>, we unregister from its <c>OnRespawn</c> event.
        if (!ReferenceEquals(_spawnable, null))
        {
            _spawnable.OnRespawn -= OnRespawn;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the object we touched is a Platform, register it in the list of platforms we are currently in
        // contact with.
        if (other.TryGetComponent<Platform>(out var platform))
        {
            PlatformsInContact.Add(platform);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // If the object we stopped touching is a Platform, unregister it from the list of platforms we are
        // currently in contact with.
        if (other.TryGetComponent<Platform>(out var platform))
        {
            PlatformsInContact.Remove(platform);
        }
    }
    
    /**
     * <summary>
     * This method is called, if this object also has a <see cref="Spawnable"/> component and its
     * <see cref="Spawnable.OnRespawn"/> event is triggered.
     *
     * We use this event to manually check for platform contacts on respawn, since the physics event functions of
     * <see cref="Chasm"/> may be called before the event functions of this behavior.
     * However, we have to register contacts with <see cref="Platform"/> before <see cref="Chasm"/> does, so we directly
     * do it on respawn using this method.
     * </summary>
     */
    private void OnRespawn(Vector3 respawnPosition, Spawnable.RespawnReason reason)
    {
        var colliderList = new List<Collider2D>();
        var contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        
        _collider.OverlapCollider(contactFilter, colliderList);

        foreach (var collider in colliderList)
        {
            OnTriggerEnter2D(collider);
        }
    }
}
