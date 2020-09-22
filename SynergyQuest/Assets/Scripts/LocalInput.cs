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

using System.Collections.Generic;
using UnityEngine;

/**
 * Implements input for a player character from the local keyboard.
 * 
 * See also `Input` interface.
 */
public class LocalInput: MonoBehaviour, Input
{
    /**
     * What keymap to use. Use different `LocalInput` instances with different layouts for multiple players
     */
    [SerializeField] private LocalKeymap keymap = default;
    /**
     * What color these controls shall report. It emulates the color assignment remote controllers undergo
     */
    [SerializeField] private PlayerColor color = default;

    /**
     * <summary>
     * What input mode these controls shall report. It emulates the input mode setting of remote controllers,
     * see also <see cref="InputMode"/>.
     * </summary>
     */
    public InputMode InputMode {
        get;
        set;
    } = InputMode.Normal;
    
    public event MenuActionTriggeredAction OnMenuActionTriggered;

    private void OnEnable()
    {
        SharedControllerState.Instance.RegisterLocalDebugInput(this);
    }
    
    private void OnDisable()
    {
        SharedControllerState.Instance.UnregisterLocalDebugInput(this);
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(keymap.PauseKey))
        {
            OnMenuActionTriggered?.Invoke(
                PauseScreenLauncher.Instance.IsPaused || InfoScreenLauncher.Instance.IsShowingInfoScreen?
                      MenuAction.ResumeGame
                    : MenuAction.PauseGame
            );
        }
        
        if (UnityEngine.Input.GetKeyDown(keymap.MapKey))
        {
            OnMenuActionTriggered?.Invoke(MenuAction.ShowMap);
        }
    }
    
    public void SetColor(PlayerColor color)
    {
        this.color = color;
    }
    
    public bool GetButton(Button button)
    {
        if (keymap.ButtonMappings.TryGetValue(button, out var keyCode))
        {
            return UnityEngine.Input.GetKey(keyCode);
        }

        return false;
    }

    public bool GetButtonDown(Button button)
    {
        if (keymap.ButtonMappings.TryGetValue(button, out var keyCode))
        {
            return UnityEngine.Input.GetKeyDown(keyCode);
        }

        return false;
    }
    
    public bool GetButtonUp(Button button)
    {
        if (keymap.ButtonMappings.TryGetValue(button, out var keyCode))
        {
            return UnityEngine.Input.GetKeyUp(keyCode);
        }

        return false;
    }

    public float GetVertical()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? 0 : UnityEngine.Input.GetAxis(keymap.VerticalAxis);
    }

    public float GetHorizontal()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? 0 : UnityEngine.Input.GetAxis(keymap.HorizontalAxis);
    }

    public float GetIMUOrientationVertical()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? UnityEngine.Input.GetAxis(keymap.VerticalAxis) : 0;
    }

    public float GetIMUOrientationHorizontal()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? UnityEngine.Input.GetAxis(keymap.HorizontalAxis) : 0;
    }

    /**
     * The built-in local controls always return the color they were initialized with.
     */
    public PlayerColor GetColor()
    {
        return color;
    }

    public void PlayVibrationFeedback(List<float> vibrationPattern)
    {
        // Vibration is not supported locally, we do nothing
    }

    /**
     * Keys on the keyboard cannot disappear. Do nothing
     */
    public void EnableButtons(params (Button, bool)[] buttonStates)
    { }

    /**
     * Keys on the keyboard cannot display a cooldown state. Do nothing.
     */
    public void SetCooldownButtons(params (Button, bool)[] buttonStates)
    { }

    /**
     * The keyboard cannot display information. Do nothing.
     */
    public void UpdatePlayerInfo(PlayerInfo info)
    { }
}
