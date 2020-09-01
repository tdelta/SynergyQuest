public static class StringExtensions
{
    /**
     * <summary>
     * Replaces any backslashes (<c>\</c>) with slashes (<c>/</c>) in a string.
     * </summary>
     * <remarks>
     * Unity only accepts forward slashes for Asset paths, hence we use this method to eliminate "\" on windows systems.
     * </remarks>
     */
    public static string WinToNixPath(this string self)
    {
        return self.Replace('\\', '/');
    }
}
