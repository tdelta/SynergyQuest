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
 * Given a source collider, this behavior will clone it at runtime and assign it to the own game object.
 * Some properties of the cloned collider can be overridden.
 *
 * This behavior is for example used for chasms, which need a separate child object with the same collider.
 * This child is set up such that only monsters collide with it.
 * This way, players can fall down a chasm, while monsters wont enter it.
 */
public class ColliderCloner : MonoBehaviour
{
    [SerializeField] private Collider2D source = default;

    /**
     * If true, the `IsTrigger` property of the cloned collider will be overriden
     */
    [SerializeField] private bool overrideIsTrigger = false;
    /**
     * The `IsTrigger` property of the cloned collider is overridden with this value,
     * if `overrideIsTrigger` is set.
     */
    [SerializeField] private bool isTrigger = false;

    private void Awake()
    {
        // Clone collider
        Collider2D copiedComponent;
        
        // The density property of colliders is special. It can only be assigned for dynamic rigidbodies using auto-mass.
        // Hence we exclude it, if these conditions do not apply:
        if (CanDensityBeCloned())
        {
            copiedComponent = source.CopyComponent( gameObject );
        }

        else
        {
            copiedComponent = source.CopyComponent(
                gameObject,
                nameof(Collider2D.density)
            );
        }

        // If enabled, override properties
        if (overrideIsTrigger)
        {
            copiedComponent.isTrigger = isTrigger;
        }
    }

    private bool CanDensityBeCloned()
    {
        var attachedRigidbody = source.attachedRigidbody;
        return attachedRigidbody != null && attachedRigidbody.bodyType == RigidbodyType2D.Dynamic && attachedRigidbody.useAutoMass;
    }
}
