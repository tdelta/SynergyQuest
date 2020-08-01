using UnityEngine;

[CreateAssetMenu(fileName = "ItemDescription", menuName = "Items/ItemDescription", order = 0)]
public class ItemDescription : ScriptableObject
{
    [SerializeField] private Item itemInstancePrefab;
    public Item ItemInstancePrefab => itemInstancePrefab;

    [SerializeField] private Button useButton;
    public Button UseButton => useButton;

    [SerializeField] private float cooldown = 0.0f;
    public float Cooldown => cooldown;
}
