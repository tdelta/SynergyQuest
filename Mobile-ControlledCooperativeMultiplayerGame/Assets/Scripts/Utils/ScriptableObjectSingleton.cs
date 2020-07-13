using System;
using UnityEngine;

public abstract class ScriptableObjectSingleton<T>: ScriptableObject
    where T: ScriptableObjectSingleton<T>
{
    // An instance of this object is lazily loaded from the Resources folder
    [NonSerialized] // <- This attribute is needed, so that changes to this variable are not saved to the resource
    private static readonly Lazy<T> _instance = new Lazy<T>(() =>
    {
        var name = typeof(T).ToString();
        var instance = Resources.Load<T>(name);

        return instance;
    });

    public static T Instance => _instance.Value;
}
