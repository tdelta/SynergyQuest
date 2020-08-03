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
}
