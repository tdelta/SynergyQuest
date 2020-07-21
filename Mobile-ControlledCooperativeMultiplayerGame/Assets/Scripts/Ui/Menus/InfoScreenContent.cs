using UnityEngine;

/**
 * Stores data for an info screen.
 * (Header, subheader, pages to display, etc.)
 *
 * You can for example supply an instance to the `Sign` object to display an info screen when reading the sign.
 *
 * It is a scriptable object, thus you can create an instance of it in the Asset creation menus.
 */
[CreateAssetMenu(fileName = "InfoScreenContent", menuName = "ScriptableObjects/InfoScreenContent")]
public class InfoScreenContent : ScriptableObject
{
    /**
     * Header to display in the info screen
     */
    public string headerText;
    
    /**
     * Slightly bigger text directly displayed beneath header
     */
    public string subHeaderText;
    
    /**
     * UI prefabs which each fill a page of the info screen.
     */
    public GameObject[] pagePrefabs;
}
