using UnityEngine;

/**
 * Base class for all items.
 * 
 * You should also create an `ItemDescription` instance, and assign your item prefab to it.
 * Then you can assign the description to `Collectible` objects in scenes.
 */
abstract public class Item : MonoBehaviour
{
    protected ItemDescription ItemDescription { get; private set; }
    
    /**
     * Called by `ItemController` when the player starts using this item after instantiating it.
     * Override `OnActivate` on sub classes to react to this event.
     *
     * @param player          the player who startet to use this item
     * @param itemDescription data related to item (prefab, button, ...)
     */
    public void Activate(PlayerController player, ItemDescription itemDescription)
    {
        ItemDescription = itemDescription;
        OnActivate(player);
    }
    
    /**
     * Called when the player starts using this item after instantiating it.
     * Should be overriden by sub classes.
     *
     * @param player          the player who startet to use this item
     */
    protected abstract void OnActivate(PlayerController player);
}
