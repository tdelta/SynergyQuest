using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/**
 * Allows to play a randomly selected sound.
 */
public class MultiSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] sounds = default;
    [SerializeField] private AudioSource audioSource = default;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOneShot()
    {
        if (SelectSound(out var sound))
        {
            audioSource.PlayOneShot(sound);
        }
    }
    
    bool SelectSound(out AudioClip sound)
    {
        if (sounds.Any())
        {
            sound = sounds[Random.Range(0, sounds.Length)];
            return true;
        }

        else
        {
            sound = null;
            return false;
        }
    }
}
