using UnityEngine;

[CreateAssetMenu(fileName = "MapPrefabSettings", menuName = "ScriptableObjects/MapPrefabSettings")]
public class MapPrefabSettings : ScriptableObjectSingleton<MapPrefabSettings>
{
    [SerializeField] public InfoScreenContent MapInfoScreenContent;
}