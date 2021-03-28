using System;
using UnityEngine;
using Utils;
using WebSocketSharp;

namespace Audio
{
    /**
     * <summary>
     * Allows all objects of the same tag to use the same shared audio source instance, instead of having one instance per object.
     * </summary>
     * <remarks>
     * Use case example:
     *     Usually one uses many instances of the retractable spikes in a level.
     *     However, if all of them have an individual audio source and play the same sound at the same time, the sound
     *     stacks and gets really loud.
     *
     *     This is resolved by having them use only one shared instance of an audio source, so only one sound is played
     *     at a time.
     *
     * Usage: Add to your prefab together with an <see cref="AudioSource"/> behavior.
     *        The settings of this <see cref="AudioSource"/> behavior in the prefab will act as a template for the
     *        shared instance that will be created.
     *
     *        Give your prefab a unique tag: All instances with the same tag will use the same audio source instance.
     * </remarks>
     */
    public class SharedAudioSource : MonoBehaviour
    {
        /**
         * Reference to the shared audio source.
         * We wrap it into <see cref="SharedResource{T}"/> since it allows us to automatically destroy the Audio source,
         * once no object is using it anymore.
         */
        private Optional<SharedResource<AudioSource>> _maybeAudioSource = Optional<SharedResource<AudioSource>>.None();

        /**
         * Provides access to the shared audio source instance
         */
        public AudioSource GetAudioSource()
        {
            Initialize();
            
            return _maybeAudioSource.Match(
                some: audioSource => audioSource.GetResource(),
                none: () => throw new ApplicationException("Could not retrieve shared audio source. Something in the initialization process must have failed.")
            );
        }

        private void Start()
        {
            Initialize();
        }

        /**
         * Same as <see cref="AudioSource.PlayOneShot"/>, but it will not play if a sound is already playing.
         */
        public void PlayOneShotIfAvailable(AudioClip clip)
        {
            var source = GetAudioSource();
            if (!source.isPlaying)
            {
                source.PlayOneShot(clip);
            }
        }

        private SharedResource<AudioSource> ConstructSharedAudioSource()
        {
            var audioSourcePrefab = GetComponent<AudioSource>();
            var audioSourceObj = new GameObject($"Shared audio of {this.tag}");
            
            // Copy settings from the AudioSource of this object into the shared audio source that is being created
            var audioSourceComponent = audioSourcePrefab.CopyComponent(
                audioSourceObj,
                "minVolume", "maxVolume", "rolloffFactor"
            );
            // Destroy the original audio source of this object
            Destroy(audioSourcePrefab);

            // Once no object is using the shared AudioSource anymore, destroy it
            return new SharedResource<AudioSource>(audioSourceComponent, audioSourceToDestroy =>
            {
                if (audioSourceToDestroy != null)
                {
                    GameObject.Destroy(audioSourceToDestroy.gameObject);
                }
            });
        }

        /**
         * Creates a audio source and assigns it to all <see cref="SharedAudioSource"/> instances with the same tag.
         * If an object with the same tag already uses a shared audio instance, do not create a new one but also start
         * using the existing one.
         */
        private void Initialize()
        {
            if (this.tag.IsNullOrEmpty())
            {
                Debug.LogError("Can not create shared audio source. All objects with a shared audio source must have a tag.");
            }
            
            // If no shared audio source has been set yet
            else if (_maybeAudioSource.IsNone())
            {
                // Lazily create one one demand
                var audioSourceRef = new Lazy<SharedResource<AudioSource>>(ConstructSharedAudioSource);

                try
                {
                    // For every other game object with the same tag...
                    foreach (var participant in GameObject.FindGameObjectsWithTag(this.tag))
                    {
                        if (!ReferenceEquals(participant, this.gameObject))
                        {
                            // If it also has the SharedAudioSource behaviour...
                            if (participant.TryGetComponent(out SharedAudioSource sharedSource))
                            {
                                // (variable which allows to abort the loop, if we find an existing shared audio source
                                //  which we can use)
                                var abort = false;
                                
                                sharedSource._maybeAudioSource.Match(
                                    // If the other object already has a shared audio source, we use it and abort the loop
                                    some: audioSource =>
                                    {
                                        abort = true;
                                        _maybeAudioSource = Optional.Some(audioSource.Copy());
                                    },
                                    // Otherwise, create a shared audio source and assign it to the other game objects too
                                    none: () =>
                                    {
                                        // ReSharper disable once AccessToDisposedClosure
                                        sharedSource._maybeAudioSource = Optional.Some(audioSourceRef.Value.Copy());
                                        if (participant.TryGetComponent(out AudioSource audioSourcePrefab))
                                        {
                                            GameObject.Destroy(audioSourcePrefab);
                                        }
                                    });

                                // Abort the loop, if we found an existing audio source instance we can use
                                // (which means all other game objects of the same tag already use it, so we dont need to continue the loop)
                                if (abort)
                                {
                                    break;
                                }
                            }
                            
                            else
                            {
                                Debug.LogError(
                                    $"A participating object of this shared audio source for {this.tag} does not contain the {nameof(SharedAudioSource)} behaviour. Object name: {participant.name}");
                            }
                        }
                    }
                    
                    // If we didnt find an existing audio source and already assigned it to ourselves,
                    //   assign the newly created shared audio instance to ourselves.
                    if (_maybeAudioSource.IsNone())
                    {
                        _maybeAudioSource = Optional.Some(audioSourceRef.Value.Copy());
                    }
                }

                finally
                {
                    // Destroy the local reference to the audio source
                    // (see also the documentation of the SharedResource class)
                    if (audioSourceRef.IsValueCreated)
                    {
                        audioSourceRef.Value.Dispose();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Once this behaviour is destroyed, also dispose of the reference to the shared audio source.
            // This will eventually also destroy the shared audio source, once no object is using it anymore.
            // (see also the documentation of the SharedResource class)
            _maybeAudioSource.Match(
                some: audioSourceRef => audioSourceRef.Dispose()
            );
            
            _maybeAudioSource = Optional<SharedResource<AudioSource>>.None();
        }
    }
}