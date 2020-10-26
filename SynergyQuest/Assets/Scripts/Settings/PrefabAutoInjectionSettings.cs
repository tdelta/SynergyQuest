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
 * This scriptable object singleton allows to define which prefabs shall be injected into every loaded scene
 * by <see cref="PrefabInjector"/>.
 * </summary>
 * <remarks>
 * An instance of this scriptable object should be stored as `Assets/Resources/PrefabAutoInjectionSettings` so that this resource
 * can be retrieved at runtime.
 * </remarks>
 */
[CreateAssetMenu(fileName = "PrefabAutoInjectionSettings", menuName = "ScriptableObjects/PrefabAutoInjectionSettings")]
public class PrefabAutoInjectionSettings : ScriptableObjectSingleton<PrefabAutoInjectionSettings>
{
    /**
     * <summary>
     * These prefabs are automatically injected into every newly loaded scene by <see cref="PrefabInjector"/>.
     * </summary>
     */
    public GameObject[] PrefabsToInject => prefabsToInject;
    [Tooltip("These prefabs are automatically injected into every newly loaded scene.")]
    [SerializeField] private GameObject[] prefabsToInject = new GameObject[0];
}
