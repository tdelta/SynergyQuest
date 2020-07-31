using UnityEngine;

public class ItemController: MonoBehaviour
{

  private PlayerController _player; 

  private void Awake()
  {
      _player = GetComponent<PlayerController>();
  }

  private void Start()
  {
      foreach(Item collectedItem in _player.CollectedItems){
        collectedItem.Player = _player;
      }
  }

  public bool HasItem(Item item)
  {
      foreach(Item collectedItem in _player.CollectedItems){
          if (collectedItem.GetType() == item.GetType()){
              return true;
          }
      }
      return false;
  }
  
  public void Collect(Item item)
  {
      _player.CollectedItems.AddLast(item);
      item.Player = _player;
  }

  private void Update()
  {
      foreach(Item item in _player.CollectedItems){
          item.Update();
      }
  }

}
