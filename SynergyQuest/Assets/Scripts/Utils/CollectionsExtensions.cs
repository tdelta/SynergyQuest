using System;
using System.Collections.Generic;
using System.Linq;

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
     * The same as Linq Select, but it filters out any null values after
     * selecting.
     */
    public static IEnumerable<TResult> SelectNotNull<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> selector
    )
    {
        return source
            .Select(selector)
            .Where(selected => !ReferenceEquals(selected, null));
    }

    /**
     * <summary>
     * Returns all but the last element of a collection.
     * Returns empty enumerable, if the collection was empty.
     * </summary>
     * <remarks>
     * The implementation currently is simple, but incredibly inefficient, do not use for large collections.
     * In .NET Standard 2.1 there is <c>SkipLast</c> which should be used instead of this, if this project is ported to
     * .NET standard 2.1 at some point in time.
     * </remarks>
     */
    public static IEnumerable<T> Init<T>(this IEnumerable<T> self)
    {
        // Incredibly inefficient, see remark in above comments:
        return self.Reverse().Skip(1).Reverse();
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

    /**
     * Returns the element with the maximum value according to the given metric.
     */
    public static TSource MaxBy<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, float> metric
    )
    {
        var (max, _) =  source.Aggregate<TSource, (TSource, float)>((default, float.NegativeInfinity), (prevMax, next) =>
        {
            var (_, prevValue) = prevMax;

            var currentValue = metric(next);

            if (currentValue > prevValue)
            {
                return (next, currentValue);
            }

            else
            {
                return prevMax;
            }
        });

        return max;
    }

    /**
     * <summary>
     * Given an array and an element, return a new array containing the elements of the old array plus the given element
     * appended at the end.
     * </summary>
     */
    public static T[] Plus<T>(this T[] array, T newElement)
    { 
        var newArray = new T[array.Length + 1];
        
        Array.Copy(array, 0, newArray, 0, array.Length);
        newArray[array.Length] = newElement;

        return newArray;
    }
    
    /**
     * <summary>
     * Given an array and an element, return a new array containing the elements of the old array plus the given element
     * prepended at the beginning of the array.
     * </summary>
     */
    public static T[] Prepend<T>(this T[] array, T newElement)
    { 
        var newArray = new T[array.Length + 1];

        newArray[0] = newElement;
        Array.Copy(array, 0, newArray, 1, array.Length);

        return newArray;
    }

    /**
     * <summary>
     * Return a new array containing the elements of the given array but with the first occurence of the given element
     * removed.
     * If the given element does not appear in the array, the given array is returned unchanged without creating a new
     * instance.
     * </summary>
     */
    public static T[] RemoveFirst<T>(this T[] array, T value)
    {
        var idx = Array.IndexOf(array, value);
        if (idx >= 0)
        {
            return array.RemoveAt(idx);
        }

        else
        {
            return array;
        }
    }

    
    /**
     * <summary>
     * Return a new array containing the elements of the given array but without the element at the given index.
     * </summary>
     */
    public static T[] RemoveAt<T>(this T[] array, int index)
    {
        var newArray = new T[array.Length - 1];
        
        Array.Copy(array, 0, newArray, 0, index);

        if (index < array.Length - 1)
        {
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
        }

        return newArray;
    }
}
