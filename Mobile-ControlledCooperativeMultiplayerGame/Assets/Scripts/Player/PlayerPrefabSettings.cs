using UnityEngine;

/**
 * Contains a reference to the prefab to be used to instantiate players.
 * See also `PlayerDataKeeper`, which accesses this scriptable object singleton to instantiate players.
 * 
 * To work correctly, an instance of this object must be saved as `Resources/PlayerPrefabSettings`.
 */
[CreateAssetMenu(fileName = "PlayerPrefabSettings", menuName = "ScriptableObjects/PlayerPrefabSettings")]
public class PlayerPrefabSettings: ScriptableObjectSingleton<PlayerPrefabSettings>
{
    /**
     * Character prefab object spawned for every controller. Must have a `PlayerController` component
     */
    [SerializeField] private PlayerController playerPrefab;
    public PlayerController PlayerPrefab => playerPrefab;
}
