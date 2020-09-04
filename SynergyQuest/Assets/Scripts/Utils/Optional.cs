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

    public static Optional<U> FromNullable<U>(U maybeValue)
        where U: class
    {
        if (ReferenceEquals(maybeValue, null))
        {
            return Optional<U>.None();
        }
        
        else
        {
            return Optional<U>.Some(maybeValue);
        }
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
        Action<T> some = null,
        Action none = null
    )
    {
        if (_hasValue)
        {
            some?.Invoke(_value);
        }

        else
        {
            none?.Invoke();
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

    /**
     * <summary>
     * If this instance is <c>None</c>, return <c>other</c>.
     * Otherwise, <c>this</c> is returned.
     * </summary>
     */
    public Optional<T> Else(Optional<T> other)
    {
        return _hasValue ? this : other;
    }

    /**
     * <summary>
     * If this instance wraps a value, the given function is applied to it, and the resulting value is returned wrapped
     * into a new optional.
     * Otherwise, an instance of <c>None</c> is returned.
     * </summary>
     */
    public Optional<U> Map<U>(Func<T, U> mapper)
    {
        if (IsSome())
        {
            return Optional<U>.Some(mapper(_value));
        }

        else
        {
            return Optional<U>.None();
        }
    }

    /**
     * <summary>
     * If this instance is <c>None</c>, return <c>alternative</c>.
     * Otherwise, the value wrapped into this instance is returned.
     * </summary>
     */
    public T ValueOr(T alternative)
    {
        return _hasValue ? _value : alternative;
    }
}
