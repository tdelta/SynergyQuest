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