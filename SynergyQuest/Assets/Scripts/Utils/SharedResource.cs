using System;
using UnityEngine;

namespace Utils
{
    /**
     * <summary>
     * Holds a reference to some resource that is shared by multiple objects.
     * Once no-one holds a reference, a destructor function is executed.
     *     This gives slightly more control on when and how a resource is disposed of than the garbage collector allows
     *     for.
     *
     * The general concept is similar to the shared pointers of C++:
     * https://en.cppreference.com/w/cpp/memory/shared_ptr
     * </summary>
     * <remarks>
     * This works by keeping track of the number of used references in a shared state object.
     * ALWAYS create copies of a reference only using <see cref="Copy"/>.
     * ALWAYS call <see cref="Dispose"/>, once you dont need a reference anymore.
     *
     * For a usage example, see <see cref="Audio.SharedAudioSource"/>.
     * </remarks>
     */
    public struct SharedResource<T>: IDisposable
        where T: class
    {
        public delegate void Destructor(T reference);
        
        private class SharedState
        {
            public readonly T Reference;
            public readonly Destructor Destructor;
            // How many references are there at the moment?
            public int Count;

            public SharedState(
                T reference,
                Destructor destructor,
                int count
            )
            {
                Reference = reference;
                Destructor = destructor;
                Count = count;
            }

            /**
             * Fallback finalizer: If for some reason the user did not use <see cref="SharedResource{T}.Dispose"/> on all
             * instances, execute the destructor callback when the shared state is garbage collected.
             */
            ~SharedState()
            {
                if (Count > 0)
                {
                    Debug.LogError($"Incorrect use of {nameof(SharedResource<T>)}: {nameof(IDisposable.Dispose)} has not been called on all instances. Fallback Activated: Destroyed resource during garbage collection.");
                    Destructor.Invoke(Reference);
                }
            }
        }
        private readonly SharedState _sharedState;

        public SharedResource(T instance, Destructor destructor)
        {
            _sharedState = new SharedState(
                reference: instance,
                destructor: destructor,
                count: 1
            );
        }

        public SharedResource(SharedResource<T> other)
        {
            if (other._sharedState.Count == 0)
            {
                throw new ApplicationException($"Can not copy dead shared resource. Did you call {nameof(Dispose)}() on the resource but kept using it?");
            }

            _sharedState = other._sharedState;
            ++_sharedState.Count;
        }

        public T GetResource()
        {
            return _sharedState.Reference;
        }

        public SharedResource<T> Copy()
        {
            return new SharedResource<T>(this);
        }

        public void Dispose()
        {
            // This reference has been marked as disposed, so we can reduce the count of references to the resource 
            --_sharedState.Count;
            // Once the count hits zero, we know there are no more references to the resource, so we can execute the destructor
            if (_sharedState.Count == 0)
            {
                _sharedState.Destructor.Invoke(_sharedState.Reference);
            }
            
            else if (_sharedState.Count < 0)
            {
                Debug.LogError($"Incorrect use of {nameof(SharedResource<T>)}: {nameof(IDisposable.Dispose)} has been called more often than there are instances. Did you copy an instance without using the {nameof(Copy)} method?");
            }
        }
    }
}