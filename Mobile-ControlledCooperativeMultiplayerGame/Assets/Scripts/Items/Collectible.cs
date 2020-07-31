using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(ContactTrigger))]
public class Collectible : MonoBehaviour
{
    [FormerlySerializedAs("item")]
    [SerializeField] private Item itemPrefab;
    public Item ItemPrefab => itemPrefab;

    public void Collect(PlayerController collector)
    {
        collector.Collect(this);
        Destroy(gameObject);
    }
}
