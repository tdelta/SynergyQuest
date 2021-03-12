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
using System.IO;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

/**
 * <summary>
 * Scriptable object singletons allow to access asset data at runtime which can not be set otherwise using the inspector.
 * </summary>
 * <remarks>
 * For example, the menu screen singletons (<see cref="PauseScreenLauncher"/> etc.) need access to their UI prefabs, but
 * since the singleton is only instantiated dynamically at runtime, the prefab can not be set using the Unity
 * inspector.
 * Instead, the scriptable object singleton <see cref="MenuPrefabSettings"/> provides access to the prefab.
 *
 * Scriptable object singletons are like normal scriptable objects, but there must be an instance of them placed
 * in the `Resources` folder of the project
 * (unless <see cref="InstantiateResourceWhenMissing"/> is set to <see cref="TrueLiteralType"/>, see below.)
 * When instantiating, the scriptable object singleton will then load this instance.
 *
 * Usage instructions:
 *
 * 1. Inherit from this class, provide the name of the subclass as type parameter
 * 2. Implement your scriptable object as usual
 * 3. Place an instance of the scriptable object with the same name as the subclass in the `Resources`
 *    folder of the project.
 * 4. Now you can access this instance anywhere at runtime by reading `MySubClass.Instance`.
 * 
 * </remarks>
 * <typeparam name="InstantiateResourceWhenMissing">
 *   Whether a resource file shall be created automatically if none exists.
 *   (Will only be created in EDITOR/Play mode, not for builds.)
 *   Default: <see cref="FalseLiteralType"/>.
 *   (boolean literal type <see cref="BooleanLiteralType"/>)
 * </typeparam>
 */
public abstract class ScriptableObjectSingleton<T, InstantiateResourceWhenMissing>: ScriptableObject
    where T: ScriptableObjectSingleton<T, InstantiateResourceWhenMissing>
    where InstantiateResourceWhenMissing: BooleanLiteralType, new()
{
    private const string AssetBasePath = "Assets/Resources/";
    
    // An instance of this object is lazily loaded from the Resources folder
    [NonSerialized] // <- This attribute is needed, so that changes to this variable are not saved to the resource
    private static readonly Lazy<T> _instance = new Lazy<T>(() =>
    {
        // We assume, that an instance of the scriptable object is placed in the resources folder, and that it
        // has the same name as its type:
        var name = typeof(T).Name;
        
        // We load and return this instance.
        var instance = Resources.Load<T>(name);
        if (instance == null)
        {
            if ((new InstantiateResourceWhenMissing()).Value)
            {
                instance = ScriptableObject.CreateInstance<T>();
                #if UNITY_EDITOR
                    AssetDatabase.CreateAsset(instance, $"{AssetBasePath}/{name}.asset"); // FIXME: Do not hardcode this path. Try to query it from Unity somehow
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                #endif
            }

            else
            {
                throw new FileNotFoundException($"No instance asset of the {name} scriptable object singleton has been created. Create an {name} resource asset with the name \"{name}\".");
            }
        }
        
        
        instance.OnInstantiate();
        
        return instance;
    });

    public static T Instance => _instance.Value;
    
    protected virtual void OnInstantiate() {}
}

public abstract class ScriptableObjectSingleton<T> : ScriptableObjectSingleton<T, FalseLiteralType>
    where T: ScriptableObjectSingleton<T, FalseLiteralType>
{ }
