using System;
using System.Linq;

/**
 * Contains an assortment of extension methods of c# internal types
 */
public static class GenericExtensions
{
    /**
     * Retrieves the next value of an enum in order of their definition inside the enum.
     * 
     * (See also https://stackoverflow.com/a/1303417)
     */
    public static TEnum NextValue<TEnum>(this TEnum currentValue)
      where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
        var lowerBoundIdx = values.GetLowerBound(0);
        var upperBoundIdx = values.GetUpperBound(0);

        var idx = Array.IndexOf(values, currentValue);

        if (idx + 1 > upperBoundIdx)
        {
            return values[lowerBoundIdx];
        }

        else
        {
            return values[idx + 1];
        }
    }
}
