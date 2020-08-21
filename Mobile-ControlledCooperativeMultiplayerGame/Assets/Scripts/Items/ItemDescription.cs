using UnityEngine;

/**
 * Stores describing data about an item:
 *
 * * item prefab
 * * button to activate the item
 * * cooldown until item can be used again
 */
[CreateAssetMenu(fileName = "ItemDescription", menuName = "Items/ItemDescription", order = 0)]
public class ItemDescription : ScriptableObject
{
    /**
     * Prefab which can be used to instantiate the described item
     */
    [SerializeField] private Item itemInstancePrefab = default;
    public Item ItemInstancePrefab => itemInstancePrefab;

    /**
     * Which button must a player press to activate the item?
     */
    [SerializeField] private Button useButton = default;
    public Button UseButton => useButton;

    /**
     * How long must the player wait after activating an item, before it can be activated again?
     * (in seconds)
     */
    [SerializeField] private float cooldown = 0.0f;
    public float Cooldown => cooldown;
}
