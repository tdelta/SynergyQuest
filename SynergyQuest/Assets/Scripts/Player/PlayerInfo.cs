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

/**
 * Information about the player that is synchronized with the controller web app.
 *
 * To synchronize an additional field, add it to the PlayerInfo interface in ControllerClient.ts
 * and make sure ControllerInput.UpdatePlayerInfo is called (maybe from the PlayerData class).
 */
[Serializable]
public class PlayerInfo
{
    public static int MAX_HEALTH_POINTS = 5;

    /**
     * Initialize to default values in order to spawn a new player.
     */
    public PlayerInfo()
    {
        HealthPoints = MAX_HEALTH_POINTS;
        Gold = 0;
    }
    
    public int HealthPoints;
    public int Gold;
}