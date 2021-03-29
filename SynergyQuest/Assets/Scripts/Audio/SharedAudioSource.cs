using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using WebSocketSharp;

namespace Audio
{
    /**
     * <summary>
     * Allows all objects with the same specified <see cref="audioSourceId"/> to use the same shared audio source
     * instance, instead of having one instance per object.
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
     *        Give a unique <see cref="audioSourceId"/>: All instances with the same id will use the same audio source instance.
     * </remarks>
     */
    public class SharedAudioSource : MonoBehaviour
    {
        /**
         * All instances with the same id will share the same audio source
         */
        [Tooltip("All instances with the same id will share the same audio source.")]
        [SerializeField] private string audioSourceId = null;

        // Registry of all SharedAudioSource instances
        // Maps the id of an audio source to the instances which use it.
        // This registry is required to distribute the same audio source reference among its users.
        public static Lazy<Dictionary<string, HashSet<SharedAudioSource>>> audioSourceUsers =
            new Lazy<Dictionary<string, HashSet<SharedAudioSource>>>(
                () => new Dictionary<string, HashSet<SharedAudioSource>>()
            );
        
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

        private void Awake()
        {
            // Register this instance as a user of the audio source with the id "audioSourceId"
            GetUsersOfSameSource().Add(this);
        }

        private void Start()
        {
            Initialize();
        }

        /**
         * <summary>
         * Get all instances of this behaviour which use the same audio source.
         * </summary>
         * <remarks>
         * If there is no entry in <see cref="audioSourceUsers"/> for our <see cref="audioSourceId"/>, an entry will be
         * created by this method.
         * </remarks>
         */
        private HashSet<SharedAudioSource> GetUsersOfSameSource()
        {
            return audioSourceUsers.Value.GetOrAdd(
                audioSourceId,
                new Lazy<HashSet<SharedAudioSource>>(() => new HashSet<SharedAudioSource>())
            );
        }

        private SharedResource<AudioSource> ConstructSharedAudioSource()
        {
            var audioSourcePrefab = GetComponent<AudioSource>();
            var audioSourceObj = new GameObject($"Shared audio of {audioSourceId}");
            
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
         * Creates a audio source and assigns it to all <see cref="SharedAudioSource"/> instances with the same
         * <see cref="audioSourceId"/>.
         * If an object with the same id already uses a shared audio instance, do not create a new one but also start
         * using the existing one.
         */
        private void Initialize()
        {
            if (this.audioSourceId.IsNullOrEmpty())
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError($"Can not create shared audio source. All objects with a shared audio source must have a {nameof(audioSourceId)}.");
            }
            
            // If no shared audio source has been set yet
            else if (_maybeAudioSource.IsNone())
            {
                // Lazily create one one demand
                var audioSourceRef = new Lazy<SharedResource<AudioSource>>(ConstructSharedAudioSource);

                try
                {
                    // For every other game object with the same id...
                    foreach (var participant in GetUsersOfSameSource())
                    {
                        if (ReferenceEquals(participant, this)) continue;
                        
                        // (variable which allows to abort the loop, if we find an existing shared audio source
                        //  which we can use)
                        var abort = false;
                            
                        participant._maybeAudioSource.Match(
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
                                participant._maybeAudioSource = Optional.Some(audioSourceRef.Value.Copy());
                                if (participant.TryGetComponent(out AudioSource audioSourcePrefab))
                                {
                                    GameObject.Destroy(audioSourcePrefab);
                                }
                            });

                        // Abort the loop, if we found an existing audio source instance we can use
                        // (which means all other game objects of the same id already use it, so we dont need to continue the loop)
                        if (abort)
                        {
                            break;
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
            // Remove this object from the list of users of the audio source
            var users = GetUsersOfSameSource();
            users.Remove(this);
            // If the list became empty, remove it completely
            if (users.IsEmpty())
            {
                audioSourceUsers.Value.Remove(audioSourceId);
            }
            
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