using System;
using UnityEngine;

/**
 * This abstract base class allows classes to implement a plain singleton pattern.
 *
 * Usage: Create a class which inherits from this one. You can then access a unique instance anywhere through the static
 * `Instance` property.
 */
public abstract class Singleton<T>
    where T: new()
{
    // Lazyily create an instance when it is first requested
    private static readonly Lazy<T> _instance = new Lazy<T>(() =>
    {
        var instance = new T();

        return instance;
    });
    
    public static T Instance => _instance.Value;
}
