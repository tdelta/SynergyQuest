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
 * <summary>
 * Moves an object periodically up and down, as if it were hovering in mid air
 * </summary>
 */
public class Hover : MonoBehaviour
{
    /**
     * How far up and down the object shall move when hovering
     */
    [SerializeField] private float range = 0.2f;

    /**
     * How fast it shall move
     */
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private int framesPerSec = 60;

    /**
     * Caches the position of the object before we started to move.
     */
    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalPosition = this.transform.position;
    }

    private void Start()
    {
        InvokeRepeating(nameof(Animate), 0.0f, 1.0f/framesPerSec);
    }

    private void Animate()
    {
        var newPosition = _originalPosition;
        newPosition.y += range * Mathf.Sin(speed * Time.time);

        this.transform.position = newPosition;
    }
}
