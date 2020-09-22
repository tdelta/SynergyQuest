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
using UnityEngine.Serialization;

/**
 * This ScriptableObject allows to store references to menu UI prefabs.
 * See also the Unity manual about scriptable objects: https://docs.unity3d.com/Manual/class-ScriptableObject.html
 * 
 * An instance of this ScriptableObject should be stored in the Assets/Resources folder, so that the singleton behaviors
 * managing menus can access it. E.g. `PauseScreenLauncher`
 */
[CreateAssetMenu(fileName = "MenuPrefabSettings", menuName = "ScriptableObjects/MenuPrefabSettings")]
public class MenuPrefabSettings : ScriptableObjectSingleton<MenuPrefabSettings>
{
    // Prefab of the pause screen UI
    public PauseScreenUi pauseScreenUiPrefab;
    
    // Prefab of the close game screen UI
    [FormerlySerializedAs("closeGameScreenUi")] public QuitGameScreenUi quitGameScreenUi;
    
    // Prefab of the info screen UI
    public InfoScreenUi infoScreenUiPrefab;

    // Prefab of the map screen UI
    [FormerlySerializedAs("mapControllerPrefab")]
    [SerializeField] private MapScreenUi mapScreenUiPrefab;
    public MapScreenUi MapScreenUiPrefab => mapScreenUiPrefab;
}
