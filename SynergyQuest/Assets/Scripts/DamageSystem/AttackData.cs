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

namespace DamageSystem
{
    /**
     * <summary>
     * Carries information about an attack, see <see cref="Attackable"/>.
     * </summary>
     */
    public class AttackData
    {
        /**
         * <summary>The game object which attacked. May be <c>null</c> if there is no specific attacker.</summary>
         */
        public GameObject attacker = null;
        
        /**
         * <summary>
         * How much damage this attack applies. Negative damage is to be interpreted as a healing effect.
         * </summary>
         */
        public int damage = 0;
        
        /**
         * <summary>
         * How many units the target of the attack shall be knocked back in <see cref="attackDirection"/>
         * </summary>
         */
        public float knockback = 0;
        
        /**
         * <summary>
         * Direction of this attack, usually its the direction from the attacker to the target.
         * Should always be a normalized vector.
         * It doesn't need to be defined (e.g. it can be None)
         * </summary>
         */
        public Optional<Vector2> attackDirection = Optional<Vector2>.None();
    }
}