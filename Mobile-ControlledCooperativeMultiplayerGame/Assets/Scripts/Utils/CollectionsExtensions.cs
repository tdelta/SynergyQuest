using System.Collections.Generic;
using UnityEngine;

public static class CollectionsExtensions
{
    public static void Resize<T>(this List<T> list, int newSize)
    {
        if (newSize > list.Count)
        {
            list.AddRange(new T[newSize - list.Count]);
        }

        else if (newSize < list.Count)
        {
            list.RemoveRange(newSize, list.Count - newSize);
        }
    }

    public static void EnsureMinSize<T>(this List<T> list, int minSize)
    {
        if (minSize > list.Count)
        {
            list.Resize(minSize);
        }
    }

    /**
     * Allows to get value from dictionary or a configurable default value if the given key is not present
     */
    public static V GetOrDefault<K, V>(this Dictionary<K, V> dictionary, K key, V defaultValue)
    {
        if (dictionary.TryGetValue(key, out var output))
        {
            return output;
        }

        else
        {
            return defaultValue;
        }
    }
}
