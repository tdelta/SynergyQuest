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

﻿using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class EndOfGameUi : MonoBehaviour
{
    private void OnEnable()
    {
        // Allow each controller to quit the game by entering a menu state
        SharedControllerState.Instance.SetGameState(GameState.Menu);
        SharedControllerState.Instance.OnMenuActionTriggered += OnMenuActionTriggered;
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.QuitGame, true),
            (MenuAction.PauseGame, false)
        );
    }

    private void OnDisable()
    {
        // The controllers shall leave the menu state when this screen is closed 
        SharedControllerState.Instance.SetGameState(GameState.Started);
        SharedControllerState.Instance.OnMenuActionTriggered -= OnMenuActionTriggered;
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.QuitGame, false),
            (MenuAction.PauseGame, true)
        );
    }

    private void OnMenuActionTriggered(MenuAction action)
    {
        if (action == MenuAction.QuitGame)
        {
            SceneController.Instance.QuitGame();
        }
    }
}
