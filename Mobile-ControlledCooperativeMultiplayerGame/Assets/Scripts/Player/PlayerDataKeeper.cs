using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

/**
 * Keeps track of all player data which is persistent across scenes, see also `PlayerData` class.
 * It also allows to instantiate player prefabs from that data. The used prefab can be set in the
 * `PlayerPrefabSettings` scriptable object singleton.
 */
public class PlayerDataKeeper: Singleton<PlayerDataKeeper>
{
    /**
     * Scene-persistent data of all players
     */
    private List<PlayerData> _playerDatas = new List<PlayerData>();

    /**
     * Determines, if the given input instance has been assigned to any player.
     */
    public bool IsInputAssignedToPlayer(Input input)
    {
        return _playerDatas.Any(data => ReferenceEquals(data.input, input));
    }

    /**
     * Instantiates a new player prefab and its across-scene-persistent data.
     *
     * @param input    input instance with which the player will be controlled
     * @param position position where the player prefab instance will be placed.
     *                 Optional parameter which defaults to (0, 0, 0).
     */
    public PlayerController InstantiateNewPlayer(Input input, Vector3 position = default)
    {
        if (IsInputAssignedToPlayer(input))
        {
            Debug.LogError("There has already a player been created with that input instance. We will continue, but this is likely an error in the game logic. Did you want to use `InstantiateExistingPlayers` instead?");
        }
        
        var data = new PlayerData(input, null);
        _playerDatas.Add(data);

        var instance = InstantiatePlayerFromData(data, position);

        return instance;
    }

    /**
     * Instantiates all players for which data is present.
     * This method should be used to instantiate existing players after loading a new scene.
     */
    public List<PlayerController> InstantiateExistingPlayers(Vector3 position = default)
    {
        return _playerDatas.Select(data => InstantiatePlayerFromData(data, position)).ToList();
    }

    /**
     * If a player prefab  instance has been created without using the methods of this singleton, it has not been
     * initialized with a PlayerData instance here.
     * Usually such instances only appear when manually placing a player in the Unity editor for debugging purposes.
     * Otherwise, a spawner should be used which will use the correct Instantiation methods.
     * 
     * This method can be called to create a such a data instance for a player prefab instance, register it and
     * initialize the prefab instance with it (the `Init` method of the PlayerController behavior is called).
     * It should only be called in the `Start` method of the `PlayerController` behavior.
     *
     * @param instance player prefab instance which shall be initialized and its data registered
     * @param input    input instance which shall control the player
     */
    public void RegisterExistingInstance(PlayerController instance, Input input)
    {
        // Check if there is already a player using that input...
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
