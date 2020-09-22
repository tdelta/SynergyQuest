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
using UnityEngine;

/**
 * This abstract base class allows classes to implement the singleton pattern in a way adapted to Unity
 * (the singleton is a game object that survives scene loads).
 *
 * Usage: Create a class which inherits from this one. You can then access a unique instance anywhere through the static
 * `Instance` property.
 */
public abstract class BehaviourSingleton<T> : MonoBehaviour
    where T: BehaviourSingleton<T>
{
    // Lazyily create an instance when it is first requested
    private static readonly Lazy<T> _instance = new Lazy<T>(() =>
    {
        // To easier differentiate the created game object from other singletons in the editor when
        // running the game, we give it the name of its behavior type as game object name:
        var name = typeof(T).ToString();
        
        var instance = new GameObject(name);
        var component = instance.AddComponent<T>();
        
        // make sure the game object can survive loading other scenes
        DontDestroyOnLoad(instance);
        
        // Give subclasses a chance to perform some setup logic on instantiation
        component.OnInstantiate();

        return component;
    });

    public static T Instance => _instance.Value;

    /**
     * Subclasses can override this to execute some logic on instantiation during the Awake phase.
     */
    protected virtual void OnInstantiate() { }
}
