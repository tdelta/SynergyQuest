using UnityEngine;
using UnityEngine.Serialization;

/**
 * This ScriptableObject allows to store references to menu UI prefabs.
 * See also the Unity manual about scriptable objects: https://docs.unity3d.com/Manual/class-ScriptableObject.html
 * 
 * An instance of this ScriptableObject should be stored in the Assets/Resources folder, so that the singleton behaviors
 * managing menus can access it. E.g. `PauseScreenLauncher`
 */
[CreateAssetMenu(fileName = "MenuPrefabSettings", menuName = "ScriptableObjects/MenuPrefabSettings")]
public class MenuPrefabSettings : ScriptableObjectSingleton<MenuPrefabSettings>
{
    // Prefab of the pause screen UI
    public PauseScreenUi pauseScreenUiPrefab;
    
    // Prefab of the close game screen UI
    [FormerlySerializedAs("closeGameScreenUi")] public QuitGameScreenUi quitGameScreenUi;
    
    // Prefab of the info screen UI
    public InfoScreenUi infoScreenUiPrefab;
}
