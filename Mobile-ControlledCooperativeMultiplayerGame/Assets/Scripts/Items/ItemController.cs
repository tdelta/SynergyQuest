using System.Collections.Generic;
using UnityEngine;

public class ItemController: MonoBehaviour
{

  private PlayerController _player; 

  private LinkedList<Item> collectedItems;

  private void Awake()
  {
      collectedItems = new LinkedList<Item>();
      _player = GetComponent<PlayerController>();
  }

  public void Collect(Item item)
  {
      collectedItems.AddLast(item);
      item.Player = _player;
  }

  private void Update()
  {
      foreach(Item item in collectedItems){
          item.Update();
      }
  }

}
