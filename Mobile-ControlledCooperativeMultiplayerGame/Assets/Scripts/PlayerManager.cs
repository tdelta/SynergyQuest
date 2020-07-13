
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlayerData
{
    public PlayerData(
        [NotNull] Input input,
        [CanBeNull] Item item
    )
    {
        _input = input;
        this.item = item;
    }
    
    private Input _input;
    public Input input => _input;
    public Item item { get; set; }
}

public class PlayerManager: Singleton<PlayerManager>
{
    private List<PlayerData> _playerDatas = new List<PlayerData>();

    public bool IsInputAssignedToPlayer(Input input)
    {
        return _playerDatas.Any(data => ReferenceEquals(data.input, input));
    }

    public PlayerController InstantiateNewPlayer(Input input, Vector3 position = default)
    {
        var data = new PlayerData(input, null);
        _playerDatas.Add(data);

        var instance = InstantiatePlayerFromData(data, position);

        return instance;
    }

    public List<PlayerController> InstantiateExistingPlayers(Vector3 position = default)
    {
        return _playerDatas.Select(data => InstantiatePlayerFromData(data, position)).ToList();
    }

    public void RegisterExistingInstance(PlayerController instance, Input input)
    {
        var playerData = _playerDatas.FirstOrDefault(data => ReferenceEquals(data.input, input));
        if (playerData != null)
        {
            Debug.LogError("A player has already been registered with this input instance. We will continue, but this might indicate an error in the game logic.");
        }

        else
        {
            playerData = new PlayerData(input, null);
            _playerDatas.Add(playerData);
        }
        
        instance.Init(playerData);
    }

    private PlayerController InstantiatePlayerFromData(PlayerData data, Vector3 position)
    {
        var instance = Object.Instantiate(PlayerPrefabSettings.Instance.PlayerPrefab, position, Quaternion.identity);
        instance.Init(data);

        return instance;
    }
}
