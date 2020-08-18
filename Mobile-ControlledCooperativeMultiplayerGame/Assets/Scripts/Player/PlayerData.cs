using JetBrains.Annotations;
using System.Collections.Generic;

public class PlayerData
{
    public PlayerData(
        [NotNull] Input input,
        [NotNull] PlayerInfo info
    )
    {
        _input = input;
        _playerInfo = info;
        
        _input.UpdatePlayerInfo(_playerInfo);
    }
    
    private LinkedList<ItemDescription> _collectedItems = new LinkedList<ItemDescription>();
    public LinkedList<ItemDescription> CollectedItems => _collectedItems;

    private PlayerInfo _playerInfo;

    private Input _input;
    public Input input => _input;
    public Item item { get; set; }

    private int _goldCounter = 0;
    public int GoldCounter {
      get => _playerInfo.Gold;
      set {
        _playerInfo.Gold = value;
        _input.UpdatePlayerInfo(_playerInfo);
      }
    }

    private int _healthPoints;
    public int HealthPoints {
      get => _playerInfo.HealthPoints;
      set {
        _playerInfo.HealthPoints = value;
        _input.UpdatePlayerInfo(_playerInfo);
      }
    }

    public string name
    {
        get
        {
            if (input is ControllerInput controllerInput)
            {
                return controllerInput.PlayerName;
            }

            else
            {
                return "Debug Player";
            }
        }
    }
}
