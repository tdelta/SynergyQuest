// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

/**
 * <summary>
 * Keeps track of all player data which is persistent across scenes, see also <see cref="PlayerData"/> class.
 * </summary>
 * <remarks>
 * Instantiates player prefabs from player data using <see cref="PlayerObjectSpawningUtils"/>.
 * The used prefab can be set in the * <see cref="PrefabSettings"/> scriptable object singleton.
 * </remarks>
 */
public class PlayerDataKeeper: Singleton<PlayerDataKeeper>
{
    /**
     * Scene-persistent data of each player
     */
    private List<PlayerData> _playerDatas = new List<PlayerData>();
    public List<PlayerData> PlayerDatas => _playerDatas;
    public int NumPlayers => _playerDatas.Count;

    /**
     * #######################
     * # Scene-persistent data shared across players
     * #######################
     */
    /**
     * The number of keys currently collected by the players
     */
    private int _numKeys = 0;
    public int NumKeys
    {
        get => _numKeys;
        set {
            _numKeys = value;
            // If the number of collected keys changes, inform subscribers
            OnNumKeysChanged?.Invoke(_numKeys);
        }
    }
    /**
     * Event which is emitted, whenever the number of keys changes, the players have collected so far.
     */
    public delegate void NumKeysChangedAction(int numKeys);
    public event NumKeysChangedAction OnNumKeysChanged;

    /**
     * The direction the door was facing which the players entered last.
     * It will be used to give the players the right orientation when entering the next room
     */
    public Direction LastDoorDirection = Direction.Down;

    /**
     * Determines, if the given input instance has been assigned to any player.
     */
    public bool IsInputAssignedToPlayer(Input input)
    {
        return _playerDatas.Any(data => ReferenceEquals(data.input, input));
    }
    
    /**
     * Instantiates all players for which data is present.
     * This method should be used to instantiate existing players after loading a new scene.
     */
    public List<PlayerController> InstantiateExistingPlayers(Vector3 targetPosition)
    {
        var newObjects = PlayerObjectSpawningUtils.InstantiatePlayerObjects(_playerDatas.Count, targetPosition);

        foreach (var (playerObject, playerData) in newObjects.Zip(_playerDatas))
        {
            playerObject.Init(playerData);
        }

        return newObjects;
    }
    
    /**
     * Instantiates a new player prefab and its across-scene-persistent data.
     *
     * @param input    input instance with which the player will be controlled
     * @param position position where the player prefab instance will be placed.
     *                 The z-coordinate will be ignored, we always set it to 0.
     *                 Optional parameter which defaults to (0, 0, 0).
     */
    public List<PlayerController> InstantiateNewPlayers(List<Input> inputs, Vector3 targetPosition)
    {
        var newObjects = PlayerObjectSpawningUtils.InstantiatePlayerObjects(inputs.Count, targetPosition);

        foreach (var (playerObject, input) in newObjects.Zip(inputs))
        {
            if (IsInputAssignedToPlayer(input))
            {
                Debug.LogError("There has already a player been created with that input instance. We will continue, but this is likely an error in the game logic. Did you want to use `InstantiateExistingPlayers` instead?");
            }
            
            RegisterInstance(playerObject, input);
        }

        return newObjects;
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
    public void RegisterInstance(PlayerController instance, Input input)
    {
        // Check if there is already a player using that input...
        var playerData = _playerDatas.FirstOrDefault(data => ReferenceEquals(data.input, input));
        if (playerData != null)
        {
            Debug.LogError("A player has already been registered with this input instance. We will continue, but this might indicate an error in the game logic.");
        }

        else
        {
            playerData = new PlayerData(input);
            _playerDatas.Add(playerData);
        }
        
        instance.Init(playerData);
    }
}
