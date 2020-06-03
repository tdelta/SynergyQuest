using System;
using UnityEngine;

/**
 * This abstract base class allows classes to implement the singleton pattern in a way adapted to Unity
 * (the singleton is a game object that survives scene loads).
 *
 * Usage: Create a class which inherits from this one. You can then access a unique instance anywhere through the static
 * `Instance` property.
 */
public abstract class Singleton<T> : MonoBehaviour
    where T: Singleton<T>
{
    // Lazyily create an instance when it is first requested
    private static readonly Lazy<GameObject> _instance = new Lazy<GameObject>(() =>
    {
        var instance = new GameObject();
        var component = instance.AddComponent<T>();
        
        // make sure the game object can survive loading other scenes
        DontDestroyOnLoad(instance);
        
        // Give subclasses a chance to perform some setup logic on instantiation
        component.OnInstantiate();

        return instance;
    });
    
    public static T Instance
    {
        get
        {
            var gameObject = _instance.Value;
            
            return gameObject.GetComponent<T>();
        }
    }

    /**
     * Subclasses can override this to execute some logic on instantiation during the Awake phase.
     */
    protected virtual void OnInstantiate() { }
}
