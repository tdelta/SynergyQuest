// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
//   David Heck (david@heck.info)
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

namespace DamageSystem
{
    /**
     * <summary>
     * Interface for different implementations of a health points store for the <see cref="Health"/> component of
     * entities.
     * 
     * Monsters use a simple variable (<see cref="DefaultHealthSaver"/>).
     * Players use a persistent <see cref="PlayerData"/> object.
     * </summary>
     * <remarks>
     * While the health of an entity is managed by the <see cref="Health"> component, the actual value is managed by a
     * different object. This allows us to persist the value between scenes for players.
     * </remarks>
     */
    public interface IHealthSaver
    {
        /**
         * Store / load a health points value
         */
        int HealthPoints { get; set; }
        
        /**
         * Initialize storage with the initial maximum health points value.
         * May be called multiple times, but only the first call shall have any effect.
         */
        void InitHealthPoints(int maxHealthPoints);
    }

    public class DefaultHealthSaver : IHealthSaver
    {   
        public int HealthPoints { get; set; }
    
        public void InitHealthPoints(int maxHealthPoints){
            HealthPoints = maxHealthPoints;
        }
    }
}