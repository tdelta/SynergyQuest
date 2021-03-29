// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

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
    
    public static bool IsEmpty<TSource>(this IEnumerable<TSource> source)
    {
        return !source.Any();
    }
    
    public static bool IsNotEmpty<TSource>(this IEnumerable<TSource> source)
    {
        return source.Any();
    }

    public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
    {
        foreach (var item in self)
        {
            action.Invoke(item);
        }
    }
    
    public static void ForEach<T>(this IEnumerable<T> self, Action<T, int> action)
    {
        int count = 0;
        foreach (var item in self)
        {
            action.Invoke(item, count);
            count++;
        }
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
     * Returns the value for the given key. If the key is not found in the map, <see cref="value"/>
     * is added to the map under the given key and returned.
     */
    public static V GetOrAdd<K, V>(this Dictionary<K, V> dictionary, K key, Lazy<V> value)
    {
        if (dictionary.TryGetValue(key, out var output))
        {
            return output;
        }

        else
        {
            dictionary.Add(key, value.Value);
            return value.Value;
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
     * Takes two enumerables and returns an enumerable of pairs of their elements.
     * Like this:
     *
     * lhs = a, b, c, ...
     * rhs = 1, 2, 3, ...
     *
     * returns (a, 1), (b, 2), (c, 3), ...
     * </summary>
     * <remarks>
     * Its like the Haskell function "zip": https://hackage.haskell.org/package/base-4.14.1.0/docs/Prelude.html#v:zip
     * </remarks>
     */
    public static IEnumerable<(A, B)> Zip<A, B>(this IEnumerable<A> lhs, IEnumerable<B> rhs)
    {
        return lhs.Zip(rhs, (left, right) => (left, right));
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
