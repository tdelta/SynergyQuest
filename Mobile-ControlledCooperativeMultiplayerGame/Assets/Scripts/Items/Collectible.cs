﻿using UnityEngine;

/**
 * An object which can be collected by players and will then give them an item.
 */
[RequireComponent(typeof(ContactTrigger))]
public class Collectible : MonoBehaviour
{
    /**
     * Description of the item which can be obtained when collecting this collectible
     */
    [SerializeField] private ItemDescription itemDescription;

    /**
     * Gives the item of this collectible to a player.
     * It is invoked by the `ContactTrigger` component, as soon as a player touches this object.
     *
     * The collectible will be destroyed afterwards.
     */
    public void Collect(PlayerController collector)
    {
        collector.Collect(itemDescription);
        Destroy(gameObject);
    }
}
