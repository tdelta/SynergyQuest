using System;
using UnityEngine;

/**
 * Scriptable object singletons allow to access asset data at runtime which can not be set otherwise using the inspector.
 * For example, the menu screen singletons (`PauseScreenLauncher` etc.) need access to their UI prefabs, but
 * since the singleton is only instantiated dynamically at runtime, the prefab can not be set using the Unity
 * inspector.
 *
 * Instead, the scriptable object singleton `MenuPrefabSettings` provides access to the prefab.
 * Scriptable object singletons are like normal scriptable objects, but there must be an instance of them placed
 * in the `Resources` folder of the project.
 * When instantiating, the scriptable object singleton will then load this instance.
 *
 * Usage instructions:
 *
 * 1. Inherit from this class, provide the name of the subclass as type parameter
 * 2. Implement your scriptable object as usual
 * 3. Place an instance of the scriptable object with the same name as the subclass in the `Resources`
 *    folder of the project.
 * 4. Now you can access this instance anywhere at runtime by reading `MySubClass.Instance`.
 */
public abstract class ScriptableObjectSingleton<T>: ScriptableObject
    where T: ScriptableObjectSingleton<T>
{
    // An instance of this object is lazily loaded from the Resources folder
    [NonSerialized] // <- This attribute is needed, so that changes to this variable are not saved to the resource
    private static readonly Lazy<T> _instance = new Lazy<T>(() =>
    {
        // We assume, that an instance of the scriptable object is placed in the resources folder, and that it
        // has the same name as its type:
        var name = typeof(T).ToString();
        // We load and return this instance.
        var instance = Resources.Load<T>(name);
        
        return instance;
    });

    public static T Instance => _instance.Value;
}
