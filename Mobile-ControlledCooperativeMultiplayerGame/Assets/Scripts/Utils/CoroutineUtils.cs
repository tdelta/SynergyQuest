using System.Collections;
using UnityEngine;

/**
 * Provides commonly used coroutines
 */
public static class CoroutineUtils
{
    public delegate void Callback();
    
    /**
     * Calls a callback after the given number of seconds.
     * Since this is a Coroutine, it must be invoked with `StartCoroutine`.
     */
    public static IEnumerator Wait(float seconds, Callback callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }
}
