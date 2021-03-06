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

using System;
using System.Collections.Generic;
using System.Linq;

/**
 * To keep track of the multitude of metadata for every controller network connection, the `ControllerServer` class
 * makes use of this class.
 *
 * See mostly the `HandleConnectionSetup` method of `ControllerServer` for an usage example.
 */
class ClientCollection
{
    // Every player gets assigned a permanent ID, which is also associated with their controller.
    // This field holds the next free player id.
    private int _nextPlayerId = 0;
    
    private Dictionary<int, ControllerInput> _playerIdsToInputs = new Dictionary<int, ControllerInput>();
    private Dictionary<string, int> _namesToPlayerIds = new Dictionary<string, int>();
    private Dictionary<int, int> _connectionIdsToPlayerIds = new Dictionary<int, int>();
    private Dictionary<int, string> _playerIdsToWebsocketIds = new Dictionary<int, string>();
    
    /**
     * Registers a new player with its controller connection meta data.
     *
     * @param name         name of the player
     * @param connectionId id of the `ControllerConnection` class instance that handles low-level network communication
     * @param websocketId  id of the websocket that is used for communication
     * @param initializer  this function will be called with the new id generated for the player and it shall produce a ControllerInput instance to be stored here
     *
     * @returns `ControllerInput` instance for the player's controller
     */
    public ControllerInput NewClient(string name, int connectionId, string websocketId, Func<int, ControllerInput> initializer)
    {
        var playerId = _nextPlayerId++;
        var input = initializer(playerId);

        _playerIdsToWebsocketIds[playerId] = websocketId;
        _namesToPlayerIds[name] = playerId;
        _connectionIdsToPlayerIds[connectionId] = playerId;
        _playerIdsToInputs[playerId] = input;

        return input;
    }

    /**
     * Retrieve the ID of a player by their name.
     *
     * @returns false if there is no player with that name registered
     */
    public bool TryGetPlayerId(string name, out int playerId)
    {
        return _namesToPlayerIds.TryGetValue(name, out playerId);
    }

    /**
     * Retrieve an `ControllerInput` wrapper for a player.
     *
     * @param playerId id of the player for which the input instance shall be retrieved
     * @param controllerInput the input instance will be written here
     * @returns false if there is no player with that id
     */
    public bool TryGetInput(int playerId, out ControllerInput controllerInput)
    {
        return _playerIdsToInputs.TryGetValue(playerId, out controllerInput);
    }

    /**
     * Retrieve the id of the websocket which is currently used to communicate with a player's controller.
     *
     * @param playerId id of the player for which the websocket id shall be retrieved
     * @param websocketId the id will be written here
     * @returns false if there is no player with that id or there is no connected websocket for them currently
     */
    public bool TryGetWebsocketId(int playerId, out string websocketId)
    {
        return _playerIdsToWebsocketIds.TryGetValue(playerId, out websocketId);
    }
    
    /**
     * Retrieve a `ControllerInput` instance by the id of the `ControllerConnection` instance that is supplying its network data.
     */
    public bool TryGetInputByConnectionId(int connectionId, out ControllerInput input)
    {
        int playerId;
        if (_connectionIdsToPlayerIds.TryGetValue(connectionId, out playerId))
        {
            return _playerIdsToInputs.TryGetValue(playerId, out input);
        }

        else
        {
            input = null;
            return false;
        }
    }

    /**
     * Returns all ControllerInput instances
     */
    public List<ControllerInput> GetInputs()
    {
        return _playerIdsToInputs.Values.ToList();
    }

    /**
     * Returns how many players are currently registered.
     */
    public int ClientCount()
    {
        return _namesToPlayerIds.Count;
    }

    /**
     * Deletes meta-data of the `ControllerConnection` and websocket instance for the controller of the specified
     * player.
     * This is called by `ControllerServer` when a connection fails / disconnects.
     *
     * As soon as a new connection is established, the corresponding meta-data can be re-registered using the
     * `SetNewConnectionInfo` method.
     */
    public void DeleteConnectionInfo(int playerId, int connectionId)
    {
        _playerIdsToWebsocketIds.Remove(playerId);
        _connectionIdsToPlayerIds.Remove(connectionId);
    }

    /**
     * Re-registers meta-data of a network connection to the controller of a specific player.
     * It is called by `ControllerServer` when a controller reconnects after a connection has been lost.
     */
    public void SetNewConnectionInfo(int playerId, int connectionId, string websocketId)
    {
        _playerIdsToWebsocketIds[playerId] = websocketId;
        _connectionIdsToPlayerIds[connectionId] = playerId;
    }
}
