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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * <summary>
 * Renders a player sprite with appropriate color and name for a given <see cref="ControllerInput"/> in the UI.
 * It is used by <see cref="LobbyMenuUi"/> to display a list of connected players.
 *
 * You must call <see cref="Init"/> after <see cref="Object.Instantiate"/>
 * </summary>
 */
public class JoinedPlayerPanel : MonoBehaviour
{
    [SerializeField] private Image playerSpriteImage = default;
    [SerializeField] private TextMeshProUGUI playerNameLabel = default;
    
    private static readonly int ShirtColorProperty = Shader.PropertyToID("_ShirtColor");

    public void Init(ControllerInput playerInput)
    {
        var modMaterial = Instantiate(playerSpriteImage.material);
        modMaterial.SetColor(ShirtColorProperty, playerInput.GetColor().ToRGB());
        playerSpriteImage.material = modMaterial;
        
        playerNameLabel.SetText(playerInput.PlayerName);
    }
}
