#if UNITY_EDITOR

using UnityEditor;

public class DebugTools
{
    [MenuItem("SynergyQuest Tools/Debug Settings")]
    public static void OpenDebugSettings()
    {
        AssetDatabase.OpenAsset(DebugSettings.Instance);
    }
}

#endif
