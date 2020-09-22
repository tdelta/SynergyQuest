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
 * <summary>
 * This behavior is intended for (moving) platforms on top of <see cref="Chasm"/>s.
 * It ensures that other objects implementing <see cref="PlatformTransportable"/> and <see cref="PhysicsEffects"/> are
 * moved along with this object.
 *
 * To actually move a platform, additionally use <see cref="WaypointControlledPlatform"/> or
 * <see cref="PlayerControlledPlatform"/>.
 * </summary>
 */
public class Platform : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only transport an object, if it has a `PlatformTransportable` component
        if (other.TryGetComponent(out PlatformTransportable _))
        {
            // If the object has a physics effects component, we set the platform as new origin which it from now on
            // shall move relative to
            if (other.TryGetComponent(out PhysicsEffects physicsEffects))
            {
                physicsEffects.SetCustomOrigin(this.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Only handle objects, if they have a `PlatformTransportable` component
        if (other.TryGetComponent(out PlatformTransportable _))
        {
            // If the object has a physics effects component...
            if (other.TryGetComponent(out PhysicsEffects physicsEffects))
            {
                // ...and it is using this platform as custom origin to move relative to, then remove the custom origin,
                // since the object has left the platform
                if (physicsEffects.CustomOrigin == this.transform)
                {
                    physicsEffects.RemoveCustomOrigin();
                }
            }
        }
    }
}
