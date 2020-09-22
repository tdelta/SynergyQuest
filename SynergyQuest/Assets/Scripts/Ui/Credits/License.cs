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
 * Controls the behavior of the "License" menu screen.
 * </summary>
 */
public class License : MonoBehaviour
{
    [Tooltip("Path to file containing the game license when the game is executed in Debug mode (Unity Editor)")]
    [SerializeField] private string pathToLicenseDebugMode = "../LICENSE.md";
    [Tooltip("Path to file containing the game license when the game is executed in production mode (standalone executable)")]
    [SerializeField] private string pathToLicenseProductionMode = "LICENSE.md";

    [SerializeField] private TextMeshProUGUI licenseTextField;

    private void Awake()
    {
        licenseTextField.SetText(LoadLicenseText());
    }

    private string LoadLicenseText()
    {
        var path = Path.Combine(
            PathUtils.GetInstallDirectory().CorrectFsSlashes(),
            (DebugSettings.Instance.DebugMode
                    ? pathToLicenseDebugMode
                    : pathToLicenseProductionMode
                ).CorrectFsSlashes()
        );

        return File.ReadAllText(path);
    }

    public void OnBackPressed()
    {
        SceneController.Instance.LoadCredits();
    }
}

