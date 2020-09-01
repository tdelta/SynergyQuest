#if UNITY_EDITOR
using System.IO;
#else
using System;
#endif

public static class PathUtils
{
    /**
     * <summary>
     * Get location where game was installed (path to directory where executable resides.
     * If the game runs in Editor mode, the current working directory is returned instead.
     *
     * The path will always use forward slashes, no backslashes. Even on Windows systems. 
     * </summary>
     */
    public static string GetInstallDirectory()
    {
        #if UNITY_EDITOR
            var installDirectory = Directory.GetCurrentDirectory();
        #else
            var installDirectory = AppDomain.CurrentDomain.BaseDirectory;
        #endif

        return installDirectory.WinToNixPath();
    }
}
