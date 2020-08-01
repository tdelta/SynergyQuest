using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemController: MonoBehaviour
{
  private PlayerController _player; 
  
  private Dictionary<ItemDescription, bool> _cooldownFlags = new Dictionary<ItemDescription, bool>();

  private void Awake()
  {
      _player = GetComponent<PlayerController>();
  }

  public bool HasItem(ItemDescription item)
  {
      return _player
          .CollectedItems
          .Any(itemDescription => itemDescription.GetType() == item.GetType());
  }
  
  public void Collect(ItemDescription itemDescription)
  {
      _player.CollectedItems.AddLast(itemDescription);
      _player.EnableGameAction(itemDescription.UseButton);
  }

  private void Update()
  {
      foreach (var itemDescription in _player.CollectedItems)
      {
          if(
              _player.Input.GetButtonDown(itemDescription.UseButton) &&
              !_cooldownFlags.GetOrDefault(itemDescription, false)
          )
          {
              _cooldownFlags[itemDescription] = true;
              StartCoroutine(
                  CoroutineUtils.Wait(itemDescription.Cooldown, () =>
                  {
                      _cooldownFlags[itemDescription] = false;
                  })
              );

              var itemInstance = Instantiate(itemDescription.ItemInstancePrefab);

              itemInstance.Activate(_player, itemDescription);
          }
      }
  }

}