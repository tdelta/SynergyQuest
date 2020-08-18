using System;
using TMPro;

public struct Optional<T>
{
    private readonly T _value;
    private readonly bool _hasValue;
    
    private Optional(T value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    public static Optional<T> Some(T value)
    {
        return new Optional<T>(value, true);
    }
    
    public static Optional<T> None()
    {
        return new Optional<T>(default(T), false);
    }

    public bool IsSome()
    {
        return this._hasValue;
    }

    public bool IsNone()
    {
        return !IsSome();
    }

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
