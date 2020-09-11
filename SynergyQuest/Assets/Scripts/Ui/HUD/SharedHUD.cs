using System;
using UnityEngine;

/**
 * <summary>
 * Screen overlay HUD which displays information shared by all players (e.g. number of collected keys).
 * </summary>
 * <remarks>
 * It is a singleton. The prefab which is used to instantiate it, can be configured in the <see cref="PrefabSettings"/>.
 * </remarks>
 */
public class SharedHUD : MonoBehaviour
{
    private static readonly Lazy<SharedHUD> _instance = new Lazy<SharedHUD>(() =>
    {
        // Instantiate te prefab defined in the `SharedHUDSettings`
        var instance = Instantiate(PrefabSettings.Instance.SharedHudPrefab);
        
        // make sure the game object can survive loading other scenes
        DontDestroyOnLoad(instance);

        return instance;
    });
    public static SharedHUD Instance => _instance.Value;
    
    /**
     * Execute `OnAwake` in Unity to ensure this singleton is instantiated.
     */
    [RuntimeInitializeOnLoadMethod]
    public static void EnsureInitialization()
    {
        var _ = Instance;
    }
}
