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
 * Applies a force on any object with a <see cref="PlayerController"/> component while it is within the bounds of this
 * object.
 * Furthermore this behavior must be assigned a color using the <see cref="Colored"/> component.
 * The force is not applied to a player, if the player either does not have a <see cref="Colored"/> component or
 * if it has a <see cref="Colored"/> component and the component has been assigned the same color as this object.
 * If this behaviour is assigned the color <see cref="PlayerColor.Any"/> or no <see cref="Colored"/> component at all,
 * all players are affected by the force.
 * </summary>
 */
[RequireComponent(typeof(PlayerController))]
public class ColoredAreaEffector : MonoBehaviour
{
    /**
     * Force to be applied
     */
    [SerializeField] private Vector2 force = default;
    
    private PlayerColor _unaffectedColor = PlayerColor.Any;
    
    /**
     * Caches handles to all forces applied to players so far, so that the forces can be removed as soon as the players
     * leave the area of effect.
     */
    private Dictionary<PlayerController, ForceEffect> _activeForces = new Dictionary<PlayerController, ForceEffect>();

    private void Awake()
    {
        if (TryGetComponent<Colored>(out var colored))
        {
            _unaffectedColor = colored.Color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (
            other.TryGetComponent(out PlayerController player) &&
            (this._unaffectedColor == PlayerColor.Any ||
            !player.Color.IsCompatibleWith(this._unaffectedColor))
        )
        {
            var forceEffect = player.PhysicsEffects.ApplyForce(force);
            
            _activeForces.Add(player, forceEffect);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (
            other.TryGetComponent(out PlayerController player) &&
            _activeForces.TryGetValue(player, out var forceEffect)
        )
        {
            player.PhysicsEffects.RemoveForce(forceEffect);
            _activeForces.Remove(player);
        }
    }
}
