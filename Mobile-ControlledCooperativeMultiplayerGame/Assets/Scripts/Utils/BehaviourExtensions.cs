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
}
