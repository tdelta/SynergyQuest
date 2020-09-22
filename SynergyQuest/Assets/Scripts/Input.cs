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

/**
 * Models the different inputs for a player character.
 *
 * The interface allows to abstract from the fact whether an input is local (keyboard) or remote (smartphone controller).
 * For a local implementation, see class `LocalInput`.
 * For a remote implementation see class `ControllerInput`.
 */
public interface Input
{
    /**
     * This event is emitted when a certain menu action is selected on the controller.
     * E.g. if the controller wants to pause the game etc.
     */
    event MenuActionTriggeredAction OnMenuActionTriggered;
 
    /**
     * Returns whether a specific button is currently pressed or not.
     * For the different button ids, see the `Button` enum.
     */
    bool GetButton(Button button);
    
    /**
     * Returns whether a specific button has been pressed during the current frame.
     * It will not return true during the next frames until the button has been pressed again.
     * 
     * For the different button ids, see the `Button` enum.
     */
    bool GetButtonDown(Button button);
    
    /**
     * Returns whether a specific button has been released during the current frame.
     * It will not return true during the next frames until the button has been pressed and released again.
     * 
     * For the different button ids, see the `Button` enum.
     */
    bool GetButtonUp(Button button);

    
    /**
     * Returns the value of the Joystick position on the vertical axis.
     * 
     * The value is in [-1; 1], at least implementors are required to only return
     * such values.
     */
    float GetVertical();
    
    /**
     * Returns the value of the Joystick position on the horizontal axis.
     * 
     * The value is in [-1; 1], at least implementors are required to only return
     * such values.
     */
    float GetHorizontal();

    /**
     * <summary>
     * Returns 3d rotation of controller device interpreted as a 2d vertical input.
     * This is usually implemented by interpreting the "roll" orientation:
     * <a href="https://en.wikipedia.org/wiki/Aircraft_principal_axes">Euler angles / Tait-Bryan angles</a>
     * <a href="https://en.wikipedia.org/wiki/Aircraft_principal_axes">Aircraft principal axes</a>
     * </summary>
     *
     * <returns>
     * The value is in [-1; 1], at least clients are required to only send such values.
     * </returns>
     */
    float GetIMUOrientationVertical();
    
    /**
     * <summary>
     * Returns 3d rotation of controller device interpreted as a 2d horizontal input.
     * This is usually implemented by interpreting the "pitch" orientation:
     * <a href="https://en.wikipedia.org/wiki/Aircraft_principal_axes">Euler angles / Tait-Bryan angles</a>
     * <a href="https://en.wikipedia.org/wiki/Aircraft_principal_axes">Aircraft principal axes</a>
     * </summary>
     *
     * <returns>
     * The value is in [-1; 1], at least clients are required to only send such values.
     * </returns>
     */
    float GetIMUOrientationHorizontal();

    InputMode InputMode { get; set; }

    /**
     * Returns the color that has been assigned to this input.
     */
    PlayerColor GetColor();

    /**
     * Enables or disables game buttons on the controller.
     * (For example, this can be used to only let the "Read" button appear if the player is currently standing in front of a sign)
     *
     * See the method description of `ControllerInput` for more details.
     */
    void EnableButtons(params (Button, bool)[] buttonStates);

    /**
     * Marks buttons on the controller as "cooling down".
     * (For example, bombs can only be used every n seconds. This method can be called to mark the bomb button as "cooling down" during that time)
     *
     * See the method description of `ControllerInput` for more details.
     */
    void SetCooldownButtons(params (Button, bool)[] buttonStates);

    /**
     * Tell the controller to vibrate. This will only have an effect if the controller
     * supports vibration.
     * 
     * @param vibrationPattern Indicates how the controller shall vibrate.
     *   The first number is the number of milliseconds to vibrate,
     *   the next is the number to milliseconds to pause,
     *   the number after that is again a number of milliseconds to vibrate and so on.
     *
     *   Hence these are numbers of milliseconds to vibrate and pause in
     *   alteration.
     */
    void PlayVibrationFeedback(List<float> vibrationPattern);

    /**
     * Updates information about the player, that a controller can display.
     * e.g. health and gold.
     *
     * @param info the PlayerInfo class contains all information that is synchronized with the controller.
     */
    void UpdatePlayerInfo(PlayerInfo info);
}

public delegate void MenuActionTriggeredAction(MenuAction action);
