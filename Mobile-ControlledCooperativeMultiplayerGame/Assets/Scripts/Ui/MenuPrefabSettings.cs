using System;
using System.Diagnostics;
using UnityEngine;

/**
 * This ScriptableObject allows to store references to menu UI prefabs.
 * See also the Unity manual about scriptable objects: https://docs.unity3d.com/Manual/class-ScriptableObject.html
 * 
 * An instance of this ScriptableObject should be stored in the Assets/Resources folder, so that the singleton behaviors
 * managing menus can access it. E.g. `PauseScreenLauncher`
 */
[CreateAssetMenu(fileName = "MenuPrefabSettings", menuName = "ScriptableObjects/MenuPrefabSettings")]
public class MenuPrefabSettings : ScriptableObject
{
    // Prefab of the pause screen UI
    public PauseScreenUi pauseScreenUiPrefab;
    
    // Prefab of the info screen UI
    public InfoScreenUi infoScreenUiPrefab;

    // An instance of this object is lazily loaded from the Resources folder
    [NonSerialized] // <- This attribute is needed, so that changes to this variable are not saved to the resource
    private static readonly Lazy<MenuPrefabSettings> _instance = new Lazy<MenuPrefabSettings>(() =>
    {
        var menuPrefabSettings = Resources.Load<MenuPrefabSettings>("MenuPrefabSettings");

        return menuPrefabSettings;
    });

    public static MenuPrefabSettings Instance => _instance.Value;
}
