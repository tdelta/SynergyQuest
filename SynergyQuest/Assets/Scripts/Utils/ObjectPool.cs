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
using UnityEngine;
using Object = UnityEngine.Object;

/**
 * <summary>
 * Object pools are used when lots of instances of a prefab must be instantiated and the cost of frequent
 * instantiations and object destructions shall be avoided.
 * For example in games where there are lots of projectiles.
 * </summary>
 *
 * <remarks>
 * <list type="bullet">
 *     <item><description>
 *         Unity does not like behaviors with generics, so a dummy child class has to be created before use:
 *         <code>
 *         class ObjectPoolT: ObjectPool&lt;T&gt; {}
 *         </code>
 *     </description></item>
 *     <item><description>
 *         initialize using a prefab with <see cref="ObjectPool.Make"/>.
 *     </description></item>
 *     <item><description>
 *         Instances can be retrieved from the pool by <see cref="GetInstance"/>.
 *     </description></item>
 *     <item><description>
 *         As soon as you don't need the instance anymore, dont destroy it, but return it using <see cref="ReturnInstance"/>.
 *     </description></item>
 * </list>
 * 
 *
 * This way, instances can be reused.
 * </remarks>
 * 
 * <example>
 * <code>
 * class MyExampleClass {
 *     [SerializeField] private MyBehaviour prefab;
 * 
 *     class Pool: MyBehaviourPool&lt;MyBehaviour&gt; {}
 *     private Pool _pool = null;
 *
 *     void Awake() {
 *         _pool = ObjectPool.Make&lt;MyBehaviourPool, MyBehaviour&gt;(this.transform, prefab);
 *     }
 *
 *     void MethodA {
 *         ...
 *         var x = _pool.GetInstance();
 *         ...
 *     }
 * 
 *     void MethodB {
 *         ...
 *         _pool.ReturnInstance(x);
 *         ...
 *     }
 * }
 * </code>
 * </example>
 */
public class ObjectPool<T>: MonoBehaviour
    where T: Component
{
    // Prefab that is instantiated by this pool
    private T prefab = null;

    // We store returned instances here until they are needed again
    private Stack<T> _unusedInstances = new Stack<T>();

    public T Prefab => prefab;

    /**
     * <summary>
     * DO NOT USE. USE <see cref="ObjectPool.Make"/> instead.
     * Sets prefab for pool.
     * </summary>
     */
    public void Init(T prefab)
    {
        if (!ReferenceEquals(this.prefab, null))
        {
            throw new InvalidOperationException("This pool already has been initialized.");
        }
        
        this.prefab = prefab;
    }
    
    /**
     * <summary>
     * Returns an instance of <see cref="prefab"/>.
     * If no pre-allocated instances are stored, a new one will be created.
     * Always return the instance with <see cref="ReturnInstance"/> when you don't need it anymore.
     * </summary>
     *
     * <param name="parent">set a parent for the instance (optional)</param>
     * <param name="activate">whether the instance shall be activated (SetActive)</param>
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
     * <summary>
     * Stores prefab instances for reuse. See also <see cref="GetInstance"/>.
     * </summary>
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
    /**
     * <summary>
     * Instantiate and initialize an object pool.
     * </summary>
     */
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
