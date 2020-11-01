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

using System.IO;
using TMPro;
using UnityEngine;

/**
 * <summary>
 * Controls the behavior of the "ExternalLibCredits" menu screen.
 * It displays which external libraries were used in the game and their licenses and disclaimers.
 * 
 * The list of JS/TS libs and the disclaimers are loaded with an external text viewer from files which are
 * auto-generated only for production builds.
 * </summary>
 */
public class ExternalLibCredits : MonoBehaviour
{
    /**
     * Opens external text viewer with list of JS/TS libraries used in this game (controller-app etc.)
     */
    public void OnViewWebLibs()
    {
        var pathToWebLibList = Path.Combine(
            PathUtils.GetInstallDirectory().CorrectFsSlashes(),
            ResourcePathSettings.Instance.WebLibsListFile
        );

        if (!File.Exists(pathToWebLibList))
        {
            Debug.LogError("Could not find auto-generated list of web libraries. It is only generated for production builds. Are you running a production build of the game?");
            return;
        }
        
        Application.OpenURL(pathToWebLibList);
    }

    /**
     * Opens external text viewer with attribution notices, licenses and disclaimers of external libraries
     * used in this game.
     */
    public void OnViewDisclaimersPressed()
    {
        var pathToDisclaimers = Path.Combine(
            PathUtils.GetInstallDirectory().CorrectFsSlashes(),
            ResourcePathSettings.Instance.LibsDisclaimerFile
        );

        if (!File.Exists(pathToDisclaimers))
        {
            Debug.LogError("Could not find auto-generated library disclaimers. It is only generated for production builds. Are you running a production build of the game?");
            return;
        }
        
        Application.OpenURL(pathToDisclaimers);
    }

    public void OnBackPressed()
    {
        SceneController.Instance.LoadCredits();
    }
}

