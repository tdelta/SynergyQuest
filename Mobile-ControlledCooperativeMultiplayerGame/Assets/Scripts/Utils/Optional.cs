using System;

/**
 * <summary>
 * An instance of this class indicates a value that may or may not be present (e.g. due to an error).
 * Furthermore, it provides properties and methods that allow to determine and deal with both cases.
 *
 * It is also useful to use it as a replacement for <c>null</c>-values of non-nullable types, e.g. structs.
 * Its basically a lightweight implementation of Haskell's Maybe: <a href="https://wiki.haskell.org/Maybe">Link</a>
 * The implementation borrows some ideas from <a href="https://github.com/nlkl/Optional">this library</a>.
 *
 * Use <see cref="Some"/> to create an instance containing a value and <see cref="None"/> to create an instance not
 * containing a value.
 * </summary>
 */
public struct Optional<T>
{
    private readonly T _value;
    private readonly bool _hasValue;
    
    private Optional(T value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    /**
     * <summary>
     * Creates an instance which wraps a value
     * </summary>
     */
    public static Optional<T> Some(T value)
    {
        return new Optional<T>(value, true);
    }
    
    /**
     * <summary>
     * Creates an instance which does not wrap a value
     * </summary>
     */
    public static Optional<T> None()
    {
        return new Optional<T>(default(T), false);
    }

    /**
     * <summary>
     * Whether this instance contains a value
     * </summary>
     */
    public bool IsSome()
    {
        return this._hasValue;
    }

    /**
     * <summary>
     * Whether this instance does not contain a value
     * </summary>
     */
    public bool IsNone()
    {
        return !IsSome();
    }

    /**
     * <summary>
     * Allows to handle the two possible cases: Value is present or not present.
     * </summary>
     * <param name="some">Action which is executed, if this instance contains a value. The value is passed as parameter.</param>
     * <param name="none">Action which is executed, if this instance does not contain a value.</param>
     */
    public void Match(
        Action<T> some,
        Action none
    )
    {
        if (_hasValue)
        {
            some(_value);
        }

        else
        {
            none();
        }
    }
    
    /**
     * <summary>
     * Allows to handle the two possible cases: Value is present or not present to derive a result value.
     * </summary>
     * <param name="some">Function which is executed, if this instance contains a value. The value is passed as parameter. It must return a result.</param>
     * <param name="none">Function which is executed, if this instance does not contain a value. It must return a result.</param>
     */
    public R Match<R>(
        Func<T, R> some,
        Func<R> none
    )
    {
        if (_hasValue)
        {
            return some(_value);
        }

        else
        {
            return none();
        }
    }
}
