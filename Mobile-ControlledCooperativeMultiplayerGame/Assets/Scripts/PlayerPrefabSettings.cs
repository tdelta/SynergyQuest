using UnityEngine;

[CreateAssetMenu(fileName = "PlayerPrefabSettings", menuName = "ScriptableObjects/PlayerPrefabSettings")]
public class PlayerPrefabSettings: ScriptableObjectSingleton<PlayerPrefabSettings>
{
    /**
     * Character prefab object spawned for every controller. Must have a `PlayerController` component
     */
    [SerializeField] private PlayerController playerPrefab;
    public PlayerController PlayerPrefab => playerPrefab;
}
