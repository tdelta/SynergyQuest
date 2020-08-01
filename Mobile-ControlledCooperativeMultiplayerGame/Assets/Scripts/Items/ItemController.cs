using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Allows a player to collect and use items.
 */
[RequireComponent(typeof(PlayerController))]
public class ItemController: MonoBehaviour
{
  private PlayerController _player; 
  
  /**
   * Stores whether a specific item can currently not be used since it has a cooldown which hast not run out
   */
  private Dictionary<ItemDescription, bool> _cooldownFlags = new Dictionary<ItemDescription, bool>();

  private void Awake()
  {
      _player = GetComponent<PlayerController>();
  }

  /**
   * Whether the player has already collected the given item
   */
  public bool HasItem(ItemDescription itemDescription)
  {
      return _player
          .CollectedItems
          .Any(otherItemDescription => otherItemDescription.name == itemDescription.name);
  }
  
  /**
   * Allows the player to use the given item.
   * This also means, that the item's usage action will be enabled on controllers
   */
  public void Collect(ItemDescription itemDescription)
  {
      if (!HasItem(itemDescription))
      {
          _player.CollectedItems.AddLast(itemDescription);
          _player.Input.SetGameAction(itemDescription.UseButton, true);
      }
  }

  private void Update()
  {
      // For every collected item
      foreach (var itemDescription in _player.CollectedItems)
      {
          // Check if its usage button is being pressed
          if(
              _player.Input.GetButtonDown(itemDescription.UseButton) &&
              !_cooldownFlags.GetOrDefault(itemDescription, false) // ...and whether it is not currently cooling down
          )
          {
              // Instantiate the item
              var itemInstance = Instantiate(itemDescription.ItemInstancePrefab, this.transform.position, Quaternion.identity);
              // Tell it, that this player is using it
              itemInstance.Activate(_player, itemDescription);
              
              // if the item has a cooldown, start it
              if (itemDescription.Cooldown > 0)
              {
                  _cooldownFlags[itemDescription] = true;
                  StartCoroutine(
                      CoroutineUtils.Wait(itemDescription.Cooldown, () =>
                      {
                          _cooldownFlags[itemDescription] = false;
                      })
                  );
              }
          }
      }
  }

}
