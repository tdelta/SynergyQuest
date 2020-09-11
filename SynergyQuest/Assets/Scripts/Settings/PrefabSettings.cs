using UnityEngine;

/**
 * <summary>
 * This scriptable object singleton allows to store references to prefabs.
 * </summary>
 * <remarks>
 * An instance of this scriptable object should be stored as `Assets/Resources/PrefabSettings` so that this resource
 * can be retrieved at runtime.
 *
 * Please note, that there are also some other settings objects for more specialized prefab settings, e.g.
 * <see cref="MenuPrefabSettings"/>
 * </remarks>
 */
[CreateAssetMenu(fileName = "PrefabSettings", menuName = "ScriptableObjects/PrefabSettings")]
public class PrefabSettings : ScriptableObjectSingleton<PrefabSettings>
{
    /**
     * <seealso cref="SharedHUD"/>
     */
    public SharedHUD SharedHudPrefab => sharedHudPrefab;
    [SerializeField] private SharedHUD sharedHudPrefab;
    
    /**
     * <summary>
     * Player character prefab object spawned for every controller. Must have a `PlayerController` component
     * </summary>
     * <remarks>
     * See also <see cref="PlayerDataKeeper"/>, which accesses this scriptable object singleton to instantiate players.
     * </remarks>
     * <seealso cref="PlayerDataKeeper"/>
     */
    public PlayerController PlayerPrefab => playerPrefab;
    [SerializeField] private PlayerController playerPrefab = default;

    /**
     * <summary>
     * Prefab used by <see cref="SpeechBubble"/> to instantiate a speech bubble game object.
     * </summary>
     */
    public SpeechBubble SpeechBubblePrefab => speechBubblePrefab;
    [SerializeField] private SpeechBubble speechBubblePrefab = default;
}
