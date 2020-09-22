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
 * Can be assigned an object which it will then move along with it at relative distance while disabling normal physics.
 * It is used by `Pushable` to move the player character along with a box when it is being pulled.
 *
 * Comment by Anton:
 *   Maybe we should use a `FixedJoint2D` instead, which Marc did in the original implementation of
 *   Sokoban boxes.
 *   However, for now it seems to work and I haven't gotten around yet to adapt the Pushable class to use a FixedJoint2D.
 *   If something goes horribly wrong here we probably should try using a Joint instead first.
 */
public class MovementBinder : MonoBehaviour
{
    /**
     * Object which is moved along this one. It is null as long as no object has been bound
     */
    private GameObject _boundObject;
    /**
     * The relative offset of the bound object from the binder.
     */
    private Vector3 _objectOffset = Vector3.zero;

    private void Update()
    {
        // If an object has been bound
        if (!ReferenceEquals(_boundObject, null)) // `ReferenceEquals` is supposedly faster than a != null check
        {
            // Move it along every frame
            _boundObject.transform.position = transform.position + _objectOffset;
        }
    }

    /**
     * Binds an object to this binder, that is, if this binder moves, the object does too at a relative distance.
     * If the object has a rigidbody, we disable that.
     */
    public void Bind(GameObject obj)
    {
        _boundObject = obj;
        // remember the relative offset of the object
        _objectOffset = _boundObject.transform.position - transform.position;

        // If there is a rigidbody on the object, temporarily disable it
        if (_boundObject.GetComponent<Rigidbody2D>() is Rigidbody2D body)
        {
            body.simulated = false;
        }

        if (_boundObject.GetComponent<Animator>() is Animator animator)
        {
            animator.enabled = false;
        }
    }

    /**
     * Unbinds an object from this binder, see also `Bind`.
     * If the object has a rigidbody, we reenable that.
     */
    public void Unbind()
    {
        if (ReferenceEquals(_boundObject, null)) return;
        
        // If there is a rigidbody on the object, enable it
        if (_boundObject.GetComponent<Rigidbody2D>() is Rigidbody2D body)
        {
            body.simulated = true;
        }
        
        if (_boundObject.GetComponent<Animator>() is Animator animator)
        {
            animator.enabled = true;
        }
        
        _boundObject = null;
    }

    /**
     * Returns true if the binder is currently manipulating an object
     */
    public bool IsActive()
    {
        return !ReferenceEquals(_boundObject, null);
    }
}
