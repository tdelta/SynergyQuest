using UnityEngine;

[RequireComponent(typeof(ContactTrigger))]
public class Collectible : MonoBehaviour
{
    [SerializeField] private ItemDescription itemDescription;

    public void Collect(PlayerController collector)
    {
        collector.Collect(itemDescription);
        Destroy(gameObject);
    }
}
