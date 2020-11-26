using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class RandomExtensions
    {
        /**
         * <summary>
         * Randomly selects an element from a collection.
         * Uniform distribution.
         * </summary>
         */
        public static Optional<T> SelectUniform<T>(IList<T> elements)
        {
            if (elements.Count > 0)
            {
                return Optional<T>.Some(elements[Random.Range(0, elements.Count - 1)]);
            }
            
            return Optional<T>.None();
        }
    }
}