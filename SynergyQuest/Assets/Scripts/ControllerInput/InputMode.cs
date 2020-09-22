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

/**
 * <summary>
 * Hint to controller, what inputs are currently expected from this specific controller by the game.
 *
 * For example, if the mode is set to <c>IMUOrientation</c>, the controller should adapt its display to give the user
 * visual feedback about the 3d orientation of the device. On the other hand, it does not need to display the joystick
 * and most buttons, since joystick input will be ignored by the game.
 *
 * This is different from <see cref="GameState"/> which is global to all controllers and also indicates the overall
 * state of the game.
 * </summary>
 */
public enum InputMode
{
    // input from all buttons, menu actions, joystick etc.
    Normal = 0,
    // orientation input from IMU sensors is expected. Controller does not need to display joystick or buttons (except the `Exit` button). Menus should still be displayed
    IMUOrientation = 1,
    /**
     * <summary>
     * The player character belonging to this controller has died, and is now undergoing the <see cref="RevivalMinigame"/>.
     * </summary>
     * <remarks>
     * The controller should display information on how to solve the minigame.
     * The controller does not need to display joystick or buttons. Menus should still be displayed
     * </remarks>
     */
    RevivalMinigame = 2
}
