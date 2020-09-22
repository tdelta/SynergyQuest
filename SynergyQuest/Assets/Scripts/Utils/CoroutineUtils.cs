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

using System.Collections;
using UnityEngine;

/**
 * Provides commonly used coroutines
 */
public static class CoroutineUtils
{
    public delegate void Callback();
    
    /**
     * Calls a callback after the given number of seconds.
     * Since this is a Coroutine, it must be invoked with `StartCoroutine`.
     */
    public static IEnumerator Wait(float seconds, Callback callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }

    /**
     * <summary>
     * Waits until the physics loop completed at least once.
     * </summary>
     */
    public static IEnumerator WaitUntilPhysicsStepCompleted(Callback callback)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        callback();
    }
}
