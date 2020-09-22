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

/**
 * Creates instances of `ControllerDebugUI` game objects when new controllers connect.
 * They are only intended to display some debug information about the state of a controller connection and sent inputs.
 */
public class ControllerDebugUISpawner : MonoBehaviour
{
    [SerializeField] private GameObject _controllerDebugUiPrefab = default;
    
    [SerializeField] private Vector3 _nextSpawnLocation = Vector3.zero;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        ControllerServer.Instance.OnNewController += OnNewController;
        foreach (var input in ControllerServer.Instance.GetInputs())
        {
            OnNewController(input);
        }
    }
    
    void OnDisable()
    {
        if (ControllerServer.Instance != null)
        {
            ControllerServer.Instance.OnNewController -= OnNewController;
        }
    }
    
    void OnNewController(ControllerInput input)
    {
        var DebugUiObject = Instantiate(_controllerDebugUiPrefab, _nextSpawnLocation, Quaternion.identity);

        var DebugUi = DebugUiObject.GetComponent<ControllerDebugUI>();
        DebugUi.SetInput(input);
        
        // Move the next spawning position to the right by the width of the last debug ui game object.
        Vector3[] worldCorners = new Vector3[4];
        DebugUiObject.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
        var width = worldCorners[2].x - worldCorners[1].x;
        
        _nextSpawnLocation.Set(
            _nextSpawnLocation.x + width,
            _nextSpawnLocation.y,
            _nextSpawnLocation.z
        );
    }
}
