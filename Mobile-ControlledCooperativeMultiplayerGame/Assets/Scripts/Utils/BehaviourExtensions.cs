using UnityEngine;

/**
 * Extension methods for MonoBehaviours
 */
public static class BehaviourExtensions
{
    /**
     * Ensures a game object does not interact with other game objects without disabling it completely.
     *
     * Currently it disables any renderer and 2d collider.
     */
    public static void MakeInvisible(this MonoBehaviour self)
    {
        var renderer = self.GetComponent<Renderer>();
        if (!ReferenceEquals(renderer, null))
        {
            renderer.enabled = false;
        }

        var collider = self.GetComponent<Collider2D>();
        if (!ReferenceEquals(collider, null))
        {
            collider.enabled = false;
        }
    }


    // TODO: Refactor to One metod with boolean argument!
    /**
     * Ensures a game object does not interact with other game objects without disabling it completely.
     *
     * Currently it enables any renderer and 2d collider.
     */
    public static void MakeVisible(this MonoBehaviour self)
    {
        var renderer = self.GetComponent<Renderer>();
        if (!ReferenceEquals(renderer, null))
        {
            renderer.enabled = true;
        }

        var collider = self.GetComponent<Collider2D>();
        if (!ReferenceEquals(collider, null))
        {
            collider.enabled = true;
        }
    }
    
    /**
     * Destroys a game object after playing a sound once.
     * Until the sound finishes playing and the game object is destroyed, it is modified to be invisible and to not
     * interact with the scene.
     *
     * Requires that the game object has an `AudioSource`.
     */
    public static void PlaySoundAndDestroy(this MonoBehaviour self)
    {
        self.MakeInvisible();

        var audioSource = self.GetComponent<AudioSource>();
        var waitTimeUntilDestroy = audioSource?.clip?.length ?? 0;
        
        audioSource.Play();
        
        Object.Destroy(self.gameObject, waitTimeUntilDestroy);
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
