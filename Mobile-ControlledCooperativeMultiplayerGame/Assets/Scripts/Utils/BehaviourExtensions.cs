using UnityEngine;

/**
 * Extension methods for MonoBehaviours
 */
public static class BehaviourExtensions
{
    /**
     * Clones a component at runtime.
     * Code by users "turbanov" and "Shaffe" from the Unity forums: http://answers.unity.com/answers/1118416/view.html
     */
    public static T CopyComponent<T>(this T original, GameObject destination)
        where T: Component
    {
        var type = original.GetType();
        
        var dst = destination.GetComponent(type) as T;
        if (!dst)
        {
            dst = destination.AddComponent(type) as T;
        }
        
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic)
            {
                continue;
            }
            
            field.SetValue(dst, field.GetValue(original));
        }

        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name")
            {
                continue;
            }
            
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }

        return dst;
    }
}
