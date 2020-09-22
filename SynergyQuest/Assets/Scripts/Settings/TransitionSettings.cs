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
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

/**
 * Contains the settings for transition animations between scenes.
 * See `TransitionController` for further explanation.
 * 
 * To work correctly, an instance of this scriptable object must be saved as `Resources/TransitionSettings`.
 */
[CreateAssetMenu(fileName = "TransitionSettings", menuName = "ScriptableObjects/TransitionSettings")]
public class TransitionSettings: ScriptableObjectSingleton<TransitionSettings>
{
    /**
     * Prefab of the UI game object which is animated
     */
    public TransitionController transitionControllerPrefab;

    /**
     * Settings for every different kind of transition animation
     */
    public TransitionTypeAnimator[] transitionTypeAnimators = {};

    /**
     * Retrieves the animator override controller with the custom animation settings for a specific type of transition
     */
    [CanBeNull]
    public AnimatorOverrideController GetAnimatorOverride(TransitionType transitionType)
    {
        // If the transition type is None, no animation shall be played and we return null
        if (transitionType == TransitionType.None)
        {
            return null;
        }

        // Otherwise, we return the animator override controller stored in the settings for the given animation type
        else
        {
            return transitionTypeAnimators
                .FirstOrDefault(data => data.transitionType == transitionType)?
                .animatorOverride;
        }
    }
}

/**
 * The different animation types supported for transitioning between scenes
 */
public enum TransitionType
{
    None, // No animation
    Fade, // Fade to/from black
    // Black overlay wiping over scene from different directions. Nice for door transitions.
    WipeDown,
    WipeUp,
    WipeLeft,
    WipeRight
}

/**
 * Stores information, on how the animation shall be changed for every transition type
 */
[Serializable]
public class TransitionTypeAnimator
{
    public TransitionType transitionType;
    [FormerlySerializedAs("animator")] public AnimatorOverrideController animatorOverride;
}
