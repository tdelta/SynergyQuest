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

using UnityEngine;

/**
 * <summary>
 * This scriptable object singleton allows to store camera settings.
 * </summary>
 * <remarks>
 * An instance of this scriptable object should be stored as `Assets/Resources/CameraSettings` so that this resource
 * can be retrieved at runtime.
 * </remarks>
 */
[CreateAssetMenu(fileName = "CameraSettings", menuName = "ScriptableObjects/CameraSettings")]
public class CameraSettings : ScriptableObjectSingleton<CameraSettings>
{
    /**
     * <summary>
     * Radius around a player character which should always be included in the field of view.
     * </summary>
     * <seealso cref="GameObjectExtensions.SetFollowedByCamera"/>
     */
    public float PlayerInclusionRadius => playerInclusionRadius;
    [Tooltip("Which radius around a player character should always be included in the field of view?")]
    [SerializeField]
    private float playerInclusionRadius = 3;
}
