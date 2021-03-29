using UnityEditor;
using UnityEngine;

namespace Utils
{
    public static class AudioSourceExtensions
    {
        /**
         * Same as <see cref="AudioSource.Play"/>, but it will not play if a sound is already playing.
         */
        public static void PlayIfAvailable(this AudioSource source)
        {
            if (!source.isPlaying)
            {
                source.Play();
            }
        }

        /**
         * Same as <see cref="AudioSource.PlayOneShot"/>, but it will not play if a sound is already playing.
         */
        public static void PlayOneShotIfAvailable(this AudioSource source, AudioClip clip)
        {
            if (!source.isPlaying)
            {
                source.PlayOneShot(clip);
            }
        }
    }
}