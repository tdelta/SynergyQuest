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
 * Controls the UI of a screen which offers the player to close the game or abort.
 * To actually launch this screen, see the `QuitGameScreenLauncher` singleton.
 *
 * See also the `MenuUi` interface for a description of its methods.
 */
public class QuitGameScreenUi : MonoBehaviour, MenuUi
{
    /**
     * Callback when the player decides to quit the game
     */
    public void OnYesPressed()
    {
        QuitGameScreenLauncher.Instance.Close();
        SceneController.Instance.QuitGame();
    }

    /**
     * Callback when the player decides not to quit the game
     */
    public void OnNoPressed()
    {
        QuitGameScreenLauncher.Instance.Close();
        PauseScreenLauncher.Instance.Launch();
    }

    public void OnLaunch()
    {
        // When this screen is opened, give remote controllers the capability to enter one of the menu actions
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.Yes, true),
            (MenuAction.No, true)
        );
    }
    
    public void OnClose()
    {
        // When this screen is closed and destroyed, remove the menu actions from remote controllers
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.Yes, false),
            (MenuAction.No, false)
        );
    }
    
    public void OnMenuActionTriggered(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.Yes:
                OnYesPressed();
                break;
            case MenuAction.No:
                OnNoPressed();
                break;
        }
    }
}
