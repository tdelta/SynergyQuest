using UnityEngine;

/**
 * An object which can be collected by players and will then give them an item.
 */
[RequireComponent(typeof(ContactTrigger))]
public class Collectible : MonoBehaviour
{
    /**
     * Description of the item which can be obtained when collecting this collectible
     */
    [SerializeField] private ItemDescription itemDescription = default;

    /**
     * Gives the item of this collectible to a player.
     * It is invoked by the `ContactTrigger` component, as soon as a player touches this object.
     *
     * If it was collected, the collectible will be destroyed.
     */
    public void Collect(PlayerController collector)
    {
        if (collector.Collect(itemDescription))
            Destroy(gameObject);
    }
}
