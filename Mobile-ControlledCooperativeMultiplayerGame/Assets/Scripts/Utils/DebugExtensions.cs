using UnityEngine;

public static class DebugExtensions
{
    /**
     * <summary>
     * The same as <see cref="Debug.LogError"/>, but it returns the given second parameter value when being evaluated.
     * Useful inside ternary expressions.
     * </summary>
     * <example>
     * <code>
     * someConditionWhichShouldBeTrue ?
     *   someValue :
     *   DebugExtensions.LogErrorWithValue("This should not happen, but we continue with some default value", defaultValue)
     * </code>
     * </example>
     * <param name="valueToReturn">This value will be returned by this function</param>
     */
    public static T LogErrorWithValue<T>(object message, T valueToReturn)
    {
        Debug.LogError(message);

        return valueToReturn;
    }
}
