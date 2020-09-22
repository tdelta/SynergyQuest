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
 * Allows to pause and resume the game.
 * Usually you do not want to use this class directly, but call the Pause UI or InfoScreen UI instead.
 * (See `PauseScreenLauncher` and `InfoScreenLauncher`)
 *
 * Usage example:
 * ```
 * PauseGameLogic.Instance.Pause();
 * ```
 */
public class PauseGameLogic : Singleton<PauseGameLogic>
{
    // Stores the value of Time.timeScale before pausing, so that it can be restored when resuming
    private float _timeScaleBeforePause;
    private bool _isPaused = false;

    public bool IsPaused => _isPaused;
    
    /**
     * Pause the game by freezing time.
     */
    public void Pause()
    {
        if (!_isPaused)
        {
            _isPaused = true;
            
            // Store the value of Time.timeScale before pausing, so that it can be restored when resuming
            _timeScaleBeforePause = Time.timeScale;
            // Set Time.timeScale to zero, which effectively pauses the game by freezing time.
            Time.timeScale = 0.0f;
        }
    }

    /**
     * Resume the game.
     */
    public void Resume()
    {
        if (_isPaused)
        {
            _isPaused = false;
            
            // Make time flow again, like it did before pausing
            Time.timeScale = _timeScaleBeforePause;
        }
    }
}
