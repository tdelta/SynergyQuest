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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerColor: int
{
    // Do not change the numeric values. The controller clients use the same values.
    Red = 0,
    Blue = 1,
    Green = 2,
    Yellow = 3,
    Any = 4 // May interact with any object.
}

public static class PlayerColorMethods
{
    /**
     * Determines whether game objects of different color may interact.
     * For example, a red box may only be pushed by the red player.
     *
     * The color `Any` may interact with any object.
     */
    public static bool IsCompatibleWith(this PlayerColor thisColor, PlayerColor otherColor)
    {
        return thisColor == PlayerColor.Any || otherColor == PlayerColor.Any || thisColor == otherColor;
    }

    /**
     * Allows to cycle through color values. This method is used whenever a new color must be assigned repeatedly, for
     * example when the game lobby assigns every player a different color.
     *
     * Order in which the values are cycled through:
     * 
     * Red -> Blue -> Green -> Yellow -> Red
     * Any -> Any
     */
    public static PlayerColor NextColor(this PlayerColor color, int numPlayers = 4)
    {
        // This code is a bit hard to read but all it does is implement a modulus
        switch (color)
        {
            case PlayerColor.Any:
                return PlayerColor.Any;
            case  PlayerColor.Red:
                if (numPlayers > 1) {
                  return PlayerColor.Blue;
                }
                // Else: Return Red (the first color)
                goto case PlayerColor.Yellow;
            case PlayerColor.Blue:
                if (numPlayers > 2) {
                  return PlayerColor.Green;
                }
                // Else: Return Red (the first color)
                goto case PlayerColor.Yellow;
            case PlayerColor.Green:
                if (numPlayers > 3) {
                  return PlayerColor.Yellow;
                }
                // Else: Return Red (the first color)
                goto case PlayerColor.Yellow;
            case PlayerColor.Yellow:
                return PlayerColor.Red;
        }

        return PlayerColor.Any;
    }

    /*
     * Get RGB values for the player colors. Can be displayed in a material
     */
    public static Color ToRGB(this PlayerColor color)
    {
        switch (color)
        {
            case PlayerColor.Red:
                return new Color(1.0f,0.52549f,0.48627f);
            case PlayerColor.Blue:
                return new Color(0.41569f,0.71765f,1.0f);
            case PlayerColor.Green:
                return new Color(0.46275f,0.82353f,0.45882f);
            case PlayerColor.Yellow:
                return new Color(1.0f,1.0f,0.41961f);
        }

        return Color.gray;
    }
}
