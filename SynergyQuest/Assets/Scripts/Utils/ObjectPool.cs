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
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

/**
 * Object pools are used when lots of instances of a prefab must be drawn and the cost of frequent instantiations
 * and object destructions shall be avoided. For example in games where there are lots of projectiles.
 *
 * Instances can be retrieved from the pool by `GetInstance`.
 * As soon as you don't need the instance anymore, dont destroy it, but return it using `ReturnInstance`.
 *
 * This way, instances can be reused.
 */
public class ObjectPool<T>: MonoBehaviour
    where T: Component
{
    // Prefab that is instantiated by this pool
    private T prefab = null;

    // We store returned instances here until they are needed again
    private Stack<T> _unusedInstances = new Stack<T>();

    public T Prefab => prefab;

    public void Init(T prefab)
    {
        if (!ReferenceEquals(this.prefab, null))
        {
            throw new RuntimeException("This pool already has been initialized.");
        }
        
        this.prefab = prefab;
    }
    
    /**
     * Returns an instance of `prefab`.
     * If no pre-allocated instances are stored, a new one will be created.
     * Always return the instance with `ReturnInstance` when you don't need it anymore.
     *
     * @param parent   set a parent for the instance (optional)
     * @param activate whether the instance shall be activated (SetActive)
     */
    public T GetInstance(Transform parent = null, bool activate = true)
    {
        T instance;
        if (_unusedInstances.Any())
        {
            instance = _unusedInstances.Pop();
            instance.transform.SetParent(parent);
        }

        else if (parent != null)
        {
            instance = Object.Instantiate(prefab, parent);
        }

        else
        {
            instance = Object.Instantiate(prefab);
        }

        if (activate)
        {
            instance.gameObject.SetActive(true);
        }

        return instance;
    }

    /**
     * Stores prefab instances for reuse. See also `GetInstance`.
     */
    public void ReturnInstance(T instance, bool deactivate = true)
    {
        instance.transform.parent = this.transform;
        if (deactivate)
        {
            instance.gameObject.SetActive(false);
        }

        _unusedInstances.Push(instance);
    }
}

public class ObjectPool
{
    public static ConcretePool Make<ConcretePool, U>(Transform parent, U prefab)
        where U: Component
        where ConcretePool: ObjectPool<U>
    {
        var obj = new GameObject();
        obj.transform.parent = parent;
        var instance = obj.AddComponent<ConcretePool>();
        instance.Init(prefab);

        return instance;
    }
}
