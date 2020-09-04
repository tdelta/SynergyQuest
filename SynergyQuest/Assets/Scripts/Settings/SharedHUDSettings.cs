using UnityEngine;

/**
 * This scriptable object singleton allows to store a reference to the prefab, which is used to instatiate the shared
 * HUD. See also `SharedHUD`.
 * 
 * An instance of this scriptable object should be stored as `Assets/Resources/SharedHUDSettings` so that the singleton
 * behavior managing the shared HUD can access it.
 */
[CreateAssetMenu(fileName = "SharedHUDSettings", menuName = "ScriptableObjects/SharedHUDSettings")]
public class SharedHUDSettings : ScriptableObjectSingleton<SharedHUDSettings>
{
    // Prefab of the shared HUD
    public SharedHUD sharedHudPrefab;
}
