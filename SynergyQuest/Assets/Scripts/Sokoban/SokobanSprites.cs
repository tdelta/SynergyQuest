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

using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Sokoban
{
    [CreateAssetMenu(fileName = "SokobanSprites", menuName = "ScriptableObjects/SokobanSprites")]
    public class SokobanSprites : ScriptableObject
    {
        [SerializeField] private Sprite redBox = default;
        [SerializeField] private Sprite blueBox = default;
        [SerializeField] private Sprite greenBox = default;
        [SerializeField] private Sprite yellowBox = default;
        [SerializeField] private Sprite anyBox = default;
        
        [SerializeField] private Sprite redSwitch = default;
        [SerializeField] private Sprite blueSwitch = default;
        [SerializeField] private Sprite greenSwitch = default;
        [SerializeField] private Sprite yellowSwitch = default;
        [SerializeField] private Sprite anySwitch = default;

        public Sprite GetBoxSprite(PlayerColor color)
        {
            switch (color)
            {
                case PlayerColor.Red:
                    return redBox;
                case PlayerColor.Blue:
                    return blueBox;
                case PlayerColor.Green:
                    return greenBox;
                case PlayerColor.Yellow:
                    return yellowBox;
                case PlayerColor.Any:
                    return anyBox;
            }

            return anyBox;
        }

        public Sprite GetSwitchSprite(PlayerColor color)
        {
            switch (color)
            {
                case PlayerColor.Red:
                    return redSwitch;
                case PlayerColor.Blue:
                    return blueSwitch;
                case PlayerColor.Green:
                    return greenSwitch;
                case PlayerColor.Yellow:
                    return yellowSwitch;
                case PlayerColor.Any:
                    return anySwitch;
            }
            
            return anySwitch;
        }
    }
}
