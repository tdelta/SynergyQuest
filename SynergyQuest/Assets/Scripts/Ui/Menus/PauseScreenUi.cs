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

﻿using UnityEngine;

/**
 * Controls the UI of the pause screen.
 * To actually launch the pause screen and pause the game, see the `PauseScreenLauncher` singleton.
 *
 * See also the `MenuUi` interface for a description of its methods.
 */
public class PauseScreenUi : MonoBehaviour, MenuUi
{
    /**
     * Callback which is invoked when the resume button on the screen is pressed
     */
    public void OnResumePressed()
    {
        PauseScreenLauncher.Instance.Close();
    }

    public void OnQuitGamePressed()
    {
        PauseScreenLauncher.Instance.Close();
        QuitGameScreenLauncher.Instance.Launch();
    }

    public void OnLaunch()
    {
        // When the pause screen UI is opened, give remote controllers the capability to close the pause screen and resume the game
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, true),
            (MenuAction.QuitGame, true)
        );
    }
    
    public void OnClose()
    {
        // When the pause screen UI is closed and destroyed, remove the "Resume" capability from remote controllers
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, false),
            (MenuAction.QuitGame, false)
        );
    }

    public void OnMenuActionTriggered(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.ResumeGame:
                OnResumePressed();
                break;
            case MenuAction.QuitGame:
                OnQuitGamePressed();
                break;
        }
    }
}
