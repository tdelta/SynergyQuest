using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

using WebSocketSharp.Server;

/**
 * Identifiers of the different buttons supported by the controller.
 * See the `GetButton` method of the `ControllerInput` class.
 */
public enum Button
{
    Attack,
    Pull
}

class ClientCollection
{
    private int _nextPlayerId = 0;
    private Dictionary<int, ControllerInput> _controllerInputs = new Dictionary<int, ControllerInput>();
    private Dictionary<string, int> _namesToPlayerIds = new Dictionary<string, int>();
    private Dictionary<int, int> _connectionIdToPlayerId = new Dictionary<int, int>();
    private Dictionary<int, string> _playerIdToWebsocketId = new Dictionary<int, string>();

    public bool TryGetPlayerId(string name, out int playerId)
    {
        return _namesToPlayerIds.TryGetValue(name, out playerId);
    }

    public bool TryGetInput(int playerId, out ControllerInput controllerInput)
    {
        return _controllerInputs.TryGetValue(playerId, out controllerInput);
    }

    public bool TryGetWebsocketId(int playerId, out string websocketId)
    {
        return _playerIdToWebsocketId.TryGetValue(playerId, out websocketId);
    }

    public ControllerInput NewClient(string name, int connectionId, string websocketId, Func<int, ControllerInput> initializer)
    {
        var playerId = _nextPlayerId++;
        var input = initializer(playerId);

        _playerIdToWebsocketId[playerId] = websocketId;
        _namesToPlayerIds[name] = playerId;
        _connectionIdToPlayerId[connectionId] = playerId;
        _controllerInputs[playerId] = input;

        return input;
    }

    public bool TryGetInputByConnectionId(int connectionId, out ControllerInput input)
    {
        int playerId;
        if (_connectionIdToPlayerId.TryGetValue(connectionId, out playerId))
        {
            return _controllerInputs.TryGetValue(playerId, out input);
        }

        else
        {
            input = null;
            return false;
        }
    }

    public int ClientCount()
    {
        return _namesToPlayerIds.Count;
    }

    public void DeleteConnectionInfo(int playerId, int connectionId)
    {
        _playerIdToWebsocketId.Remove(playerId);
        _connectionIdToPlayerId.Remove(connectionId);
    }

    public void SetNewConnectionInfo(int playerId, int connectionId, string websocketId)
    {
        _playerIdToWebsocketId[playerId] = websocketId;
        _connectionIdToPlayerId[connectionId] = playerId;
    }
}

/**
 * This class allows to receive inputs from remote controllers over the network
 * and also share some data with the controllers (e.g. assign them a color).
 * Its usage is similar to the `Input` class of Unity, though much simpler.
 *
 * Right now, only one remote controller is supported.
 *
 * To use this class, create a game object and assign this script to it.
 * You can then give your character control script a reference to this game
 * object. That's it :).
 * (You also should ensure that the Update function of this script is
 *  executed before all other ones in the Script Execution Order project
 *  settings, but this has already been done.)
 * 
 * You can then for example check whether the `Attack` button is pressed like
 * this:
 * `controllerInput.GetButton(Button.Attack)`
 *
 * And you can retrieve the values of the horizontal or vertical joystick
 * axes like this:
 * `controllerInput.Vertical()`
 *
 * You can assign a player color like this:
 * `controllerInputCo.SetColor("#c0ffee");`
 *
 * Internally, this class uses a websocket connection to the controller over
 * which JSON messages are exchanged. For the format of the JSON messages,
 * see the `Message` class hierarchy.
 * Since the native C# WebSocket functionalities which are required for
 * servers (a compatible HTTPListener implementation) do not ship with .NET
 * runtime of the current version of Unity (at least not on Linux), we use
 * the MIT licensed library websocket-sharp for this.
 */
public class ControllerServer : MonoBehaviour
{
    [SerializeField] private int _maxNumPlayers = 2;
    
    // Accepts websocket connections
    private WebSocketServer _wss;
    // Keeps track of all connections
    private WebSocketSessionManager _wssm;
    private int _nextNumericConnectionId = 0;

    private ClientCollection _clients = new ClientCollection();
    
    // Since the websocket server runs on its own thread, it acts like a
    // producer which puts received messages in this thread-safe queue.
    //
    // Then, the main thread of Unity acts as a consumer in the `Update`
    // function.
    private ConcurrentQueue<ConnectionUpdate> _incomingConnectionUpdates = new ConcurrentQueue<ConnectionUpdate>();
    
    // Port the game will listen on for connections from controllers
    private const short _port = 4242;

    public delegate void NewControllerAction(ControllerInput input);
    public event NewControllerAction OnNewController;
    
    /**
     * Starts listening for websocket connections of controllers, as soon as
     * this game object is enabled.
     */
    void Awake()
    {
        Log($"Starting HTTP server on port {_port}...");
        
        // Bind to all local addresses (0.0.0.0)
        _wss = new WebSocketServer($"ws://0.0.0.0:{_port}");
        
        // The connection class will handle the connection on a different
        // thread.
        _wss.AddWebSocketService<Connection>(
            "/sockets",      // we expect connections on the /sockets path
            connection =>
            {
                // we use this initialization function to pass the message queue
                // to the connection worker and remember the id of the connection
                connection.Initialize(_nextNumericConnectionId++, _incomingConnectionUpdates);
                // ^ NOTICE: As far as I understand it, reference assignment should be
                //           thread-safe. However, if strange things happen, we should look
                //           here first.
                Log("A new connection has been established.");
            });
        
        // Now we need to get a handle to the session manager which
        // keeps track of all connections, so that we can use it later
        // to send data
        WebSocketServiceHost serviceHost;
        if (_wss.WebSocketServices.TryGetServiceHost("/sockets", out serviceHost))
        {
            _wssm = serviceHost.Sessions;
        }

        else
        {
            Log("Could not get a hold of the websocket service host. This means we can not send any messages.");
        }
        
        _wss.Start();
    }

    private void HandleConnectionSetup(string name, int connectionId, string websocketId)
    {
        int playerId;
        // check if there is already a player of this name...
        if (_clients.TryGetPlayerId(name, out playerId))
        {
            ControllerInput input;
            if (_clients.TryGetInput(playerId, out input))
            {
                // if the controller of that player is still connected, deny the connection
                if (input.IsConnected())
                {
                    SendToByWebsocketId(websocketId, new Message.NameTakenMessage(name));
                }

                // otherwise, its a controller which tries to reconnect, so we accept it
                else
                {
                    _clients.SetNewConnectionInfo(playerId, connectionId, websocketId);
                    input.SetStatus(ConnectionStatus.Connected);
                    SendToByWebsocketId(websocketId, new Message.NameOkMessage());
                }
            }

            else
            {
                LogError("There is an established player id without an input object. This should never happen and is a programming error.");
            }
        }

        // ...otherwise, this is an entirely new player
        else
        {
            // if we have not yet reached the maximum number of players, we can accept the new controller
            if (_clients.ClientCount() < _maxNumPlayers)
            {
                var newInput = _clients.NewClient(
                    name, 
                    connectionId,
                    websocketId,
                    newPlayerId =>
                    {
                        var input = new ControllerInput(newPlayerId, this);
                        input.SetStatus(ConnectionStatus.Connected);

                        return input;
                    }
                );
                
                // FIXME: Announce new player over event
                
                Log($"Accepted new player of name {name}");
                SendToByWebsocketId(websocketId, new Message.NameOkMessage());
                
                OnNewController?.Invoke(newInput);
            }

            // otherwise we have to deny establishing a connection
            else
            {
                Log("Controller tried to connect, but the maximum number of players has already been reached.");
                SendToByWebsocketId(websocketId, new Message.MaxPlayersReachedMessage());
            }
        }
    }

    /**
     * Checks every frame, whether inputs arrived and remembers them in case
     * some game object asks for them.
     */
    void Update()
    {
        ConnectionUpdate updateBuffer; // buffer to store an update
        
        // For every update that has been received until this frame
        while (_incomingConnectionUpdates.TryDequeue(out updateBuffer))
        {
            updateBuffer.Match(new ConnectionUpdate.Matcher() {
                NameUpdate = update => HandleConnectionSetup(update.name, update.connectionId, update.websocketId),
                
                MessageUpdate = update =>
                {
                    ControllerInput input;
                    if (_clients.TryGetInputByConnectionId(update.connectionId, out input))
                    {
                        input.HandleMessage(update.message);
                    }

                    else if (!(update.message is Message.NameMessage))
                    {
                        LogError("Got a message from a connection which is not associated with any input. This should never happen and is a programming error.");
                    }
                },
                
                DisconnectUpdate = update =>
                {
                    ControllerInput input;
                    if (_clients.TryGetInputByConnectionId(update.connectionId, out input))
                    {
                        _clients.DeleteConnectionInfo(input.PlayerId, update.connectionId);
                        input.SetStatus(ConnectionStatus.NotConnected);
                    }
                }
            });
        }
    }
    
    // FIXME: Find a way to only make this accessible to the ControllerInput class
    public void SendTo(int playerId, Message msg)
    {
        string websocketId;
        if (_clients.TryGetWebsocketId(playerId, out websocketId))
        {
            SendToByWebsocketId(websocketId, msg);
        }

        else
        {
            throw new ApplicationException("Tried to send to player who is not connected.");
        }
    }

    private void SendToByWebsocketId(string websocketId, Message msg)
    {
        this._wssm.SendTo(
            msg.ToJson(), websocketId
        ); // synchronous sending! See also note below 
        
        // NOTE: There is also a method SendToAsync, however, if I understand the source code of websocket-sharp
        //       correctly, while there is a lock for thread-safety for sending in the WebSocket implementation, there
        //       is no guarantee of order for the Async method.
        //       (Since the asynchronicity is implemented on top of BeginInvoke:
        //        https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/calling-synchronous-methods-asynchronously
        //       )
        //       Hence, we use the synchronous send method here. This could result in a drop of frame rate when called
        //       often, however, SendTo is probably called only once per game session for every player to set a color.
        //       If in the future, we want to send some data frequently, we could use a dedicated worker thread for this
        //       which ensures sending order by consuming a queue.
        //
        //       By the way, since WebSocket is implemented on top of TcpClient which is implemented on top of
        //       NetworkStream, reading and writing at the same time should be thread-safe.
        //       However, no two threads should send at the same time and no two threads should be receiving at the same
        //       time: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=netcore-3.1
    }

    private void OnDestroy()
    {
        _wss.Stop();
    }

    private void Log(String str) {
        Debug.Log("ControllerInput: " + str);
    }
    
    private void LogError(String str) {
        Debug.LogError("ControllerInput: " + str);
    }
}
