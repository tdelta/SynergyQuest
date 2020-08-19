using UnityEngine;

/**
 * Extension methods for MonoBehaviours
 */
public static class BehaviourExtensions
{
    /**
     * Ensures a game object does / does not interact with other game objects.
     * 
     * Unlike SetActive, it does not disable all functionality of an object.
     * Instead, currently it disables any renderer and 2d collider.
     *
     * @param visible true iff the object shall interact / be visible
     */
    public static void SetVisibility(this GameObject self, bool visible)
    {
        if (self.GetComponent<Renderer>() is Renderer renderer)
        {
            renderer.enabled = visible;
        }

        if (self.GetComponent<Collider2D>() is Collider2D collider)
        {
            collider.enabled = visible;
        }

        foreach (var interactive in self.GetComponents<Interactive>())
        {
            interactive.enabled = visible;
        }

        if (self.CompareTag("Player"))
        {
            self.SetFollowedByCamera(visible);
        }
    }

    public static void MakeInvisible(this GameObject self)
    {
        self.SetVisibility(false);
    }

    public static void MakeVisible(this GameObject self)
    {
        self.SetVisibility(true);
    }

    /**
     * <summary>
     * Ensures that a physics controlled entity can no longer move due to physics.
     * Can also unfreeze by setting the parameter to <c>false</c>
     * </summary>
     * <seealso cref="UnFreeze"/>
     */
    public static void Freeze(this GameObject self, bool freeze = true)
    {
        if (self.GetComponent<Rigidbody2D>() is Rigidbody2D body && body != null)
        {
            body.simulated = !freeze;
        }
    }
    
    /**
     * <seealso cref="Freeze"/>
     */
    public static void UnFreeze(this GameObject self)
    {
        self.Freeze(false);
    }
    
    /**
     * Destroys a game object after playing a sound once.
     * Until the sound finishes playing and the game object is destroyed, it is modified to be invisible and to not
     * interact with the scene.
     *
     * Requires that the game object has an `AudioSource`.
     */
    public static void PlaySoundAndDestroy(this GameObject self)
    {
        self.MakeInvisible();

        var audioSource = self.GetComponent<AudioSource>();
        var waitTimeUntilDestroy = audioSource?.clip?.length ?? 0;
        
        audioSource.Play();
        
        Object.Destroy(self, waitTimeUntilDestroy);
    }

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
