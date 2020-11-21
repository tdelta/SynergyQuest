using System.Linq;
using System.Reflection;
using UnityEngine;

/**
 * Extension methods for MonoBehaviours
 */
public static class BehaviourExtensions
{
    /**
     * <summary>
     * Clones a component at runtime.
     * Code by users "turbanov" and "Shaffe" from the Unity forums: http://answers.unity.com/answers/1118416/view.html
     * </summary>
     * <param name="original"><see cref="Component"/> which shall be cloned.</param>
     * <param name="destination"><see cref="GameObject"/> to which the clone shall be added.</param>
     * <param name="propertiesToExclude">Names of properties whose value shall not be cloned.</param>
     */
    public static T CopyComponent<T>(
        this T original,
        GameObject destination,
        params string[] propertiesToExclude
    )
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
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name" || propertiesToExclude.Contains(prop.Name))
            {
                continue;
            }
            
            prop.SetValue(dst, prop.GetValue(original, null), null);
        }

        return dst;
    }
    
    
    /**
     * <summary>
     * Invokes the method <c>OnValidate</c> on all other components of the same game object, even if it is private.
     * However, `OnValidate` will not be invoked on the component calling this method.
     *
     * <c>OnValidate</c> must not declare any parameters on all components.
     * </summary>
     */
    public static void ValidateOtherComponents<T>(this T self)
        where T : MonoBehaviour
    {
        foreach (var component in self.GetComponents<MonoBehaviour>())
        {
            if (component != self)
            {
                var onValidate = component.GetType().GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);

                onValidate?.Invoke(component, new object[0]);
            }
        }
    }

    public static TInterface[] GetComponentsByInterface<TInterface>(Behaviour self)
    {
        return self
            .GetComponents<MonoBehaviour>()
            .OfType<TInterface>()
            .ToArray();
    }
}
