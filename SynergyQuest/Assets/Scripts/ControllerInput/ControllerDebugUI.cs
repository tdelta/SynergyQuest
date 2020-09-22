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

using System;
using TMPro;
using UnityEngine;

/**
 * Displays remote inputs from the ControllerInput class live as an UI element for debugging purposes.
 */
public class ControllerDebugUI : MonoBehaviour
{
    private ControllerInput _input;

    [SerializeField] private TextMeshProUGUI _connectionStatusLabel = default;
    
    [SerializeField] private TextMeshProUGUI _verticalValLabel = default;
    
    [SerializeField] private TextMeshProUGUI _horizontalValLabel = default;
    
    [SerializeField] private TextMeshProUGUI _attackValLabel = default;
    
    [SerializeField] private TextMeshProUGUI _pullValLabel = default;

    public void SetInput(ControllerInput input)
    {
        this._input = input;
        RegisterCallbacks();
        SetConnectionStatusMsg();
    }
    
    private void OnDisconnect(ControllerInput input)
    {
        SetConnectionStatusMsg();
    }

    private void OnReconnect(ControllerInput input)
    {
        SetConnectionStatusMsg();
    }

    public void OnSetColorButton()
    {
        _input.SetColor(PlayerColor.Red);
    }

    private void OnEnable()
    {
        if (_input != null)
        {
            RegisterCallbacks();
            SetConnectionStatusMsg();
        }
    }

    private void RegisterCallbacks()
    {
        _input.OnReconnect += OnReconnect;
        _input.OnDisconnect += OnDisconnect;
    }

    private void OnDisable()
    {
        _input.OnReconnect -= OnReconnect;
        _input.OnDisconnect -= OnDisconnect;
    }

    // Update is called once per frame
    void Update()
    {
        _attackValLabel.SetText(_input.GetButton(Button.Attack).ToString());
        _pullValLabel.SetText(_input.GetButton(Button.Pull).ToString());
        _verticalValLabel.SetText((Math.Truncate(_input.GetVertical() * 100)/100).ToString());
        _horizontalValLabel.SetText((Math.Truncate(_input.GetHorizontal() * 100)/100).ToString());
    }

    private void SetConnectionStatusMsg()
    {
        string connectionStatusMsg;
        switch (_input.ConnectionStatus)
        {
            case ConnectionStatus.Connected:
                connectionStatusMsg = "Connected";
                break;
            case ConnectionStatus.NotConnected:
                connectionStatusMsg = "Lost Connection";
                break;
            default:
                connectionStatusMsg = "Unkown State";
                break;
        }
        
        _connectionStatusLabel.SetText(connectionStatusMsg);
    }
}
