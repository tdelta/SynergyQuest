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
 * Whenever a new scene loads, this singleton initializes instances of all prefabs specified in <see cref="PrefabAutoInjectionSettings"/>.
 * </summary>
 * <remarks>
 * The instances will be ready before the <c>Start</c> phase but not before the <c>OnEnable</c> phase.
 * </remarks>
 */
public class PrefabInjector : BehaviourSingleton<PrefabInjector>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void EnsureInitialization()
    {
        var _ = Instance;
    }

    protected override void OnInstantiate()
    {
        SceneController.Instance.OnNewSceneLoading += OnNewSceneLoading;
    }

    private void OnDestroy()
    {
        var controllerInstance = SceneController.Instance;
        if (controllerInstance != null)
        {
            controllerInstance.OnNewSceneLoading -= OnNewSceneLoading;
        }
    }

    private void OnNewSceneLoading()
    {
        foreach (var prefab in PrefabAutoInjectionSettings.Instance.PrefabsToInject)
        {
            Instantiate(prefab);
        }
    }
}
