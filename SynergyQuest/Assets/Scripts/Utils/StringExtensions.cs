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
    
    /**
     * <summary>
     * Replaces any slashes (<c>/</c>) with backslashes (<c>\</c>) in a string.
     * </summary>
     */
    public static string NixToWinPath(this string self)
    {
        return self.Replace('/', '\\');
    }

    /**
     * <summary>
     * Replaces any slashes (<c>/</c>) with backslashes (<c>\</c>) in a string on Windows systems.
     * Replaces any backslashes (<c>\</c>) with slashes (<c>/</c>) on all other systems.
     * </summary>
     */
    public static string CorrectFsSlashes(this string self)
    {
        #if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
            return self.NixToWinPath();
        #else
            return self.WinToNixPath();
        #endif
    }
}
