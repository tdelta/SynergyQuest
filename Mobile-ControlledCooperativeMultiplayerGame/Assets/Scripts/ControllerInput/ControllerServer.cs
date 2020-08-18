using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

/**
 * This class accepts (websocket) connections of game controllers and
 * manages these connections.
 * For every fully connected controller, a `ControllerInput` instance is
 * emitted by the `OnNewController` event of this class.
 * You should listen to the event and use the produced input instances in your
 * game logic since they give access to the individual game controllers
 * on a high-level.
 *
 * Furthermore, a websocket service is provided offering diagnostic information
 * about the game, see also the `DiagnosticsConnection` class.
 * 
 * Internally, this class uses a websocket connection to the controller over
 * which JSON messages are exchanged. For the format of the JSON messages,
 * see the `Message` class hierarchy and the `DiagnosticsMessage` class.
 * Since the native C# WebSocket functionalities which are required for
 * servers (a compatible HTTPListener implementation) do not ship with the .NET
 * runtime of the current version of Unity (at least not on Linux), we use
 * the MIT licensed library websocket-sharp for this.
 * 
 * You should ensure that the Update function of this script is
 * executed before all other ones in the Script Execution Order project
 * settings, but that should have already been done.
 */
public class ControllerServer : BehaviourSingleton<ControllerServer>
{
    // The maximum number of players/controllers we allow to connect
    [SerializeField] private int _maxNumPlayers = 4;
    
    // Accepts websocket connections and serves the controller web page (in production mode)
    private BaseServer _baseServer;
    // Keeps track of all connections
    private WebSocketSessionManager _wssm;
    // Every connection gets a numeric id. This is the next free id
    private int _nextNumericConnectionId = 0;

    // Meta-data for each connection is stored using this helper class
    private ClientCollection _clients = new ClientCollection();
    
    // Since the websocket server runs on its own thread, it acts like a
    // producer which puts received messages in this thread-safe queue.
    //
    // Then, the main thread of Unity acts as a consumer in the `Update`
    // function.
    private ConcurrentQueue<ConnectionUpdate> _incomingConnectionUpdates = new ConcurrentQueue<ConnectionUpdate>();
    
    // Port the game will listen on for connections from controllers
    private const short _port = 4242;

    /**
     * As soon as a new controller fully connects, this event will emit a `ControllerInput` instance.
     */
    public event NewControllerAction OnNewController;
    public delegate void NewControllerAction(ControllerInput input);
    
    /**
     * Starts listening for websocket connections of controllers, as soon as
     * this game object is awakened.
     */
    void Awake()
    {
        _baseServer = new BaseServer();
        
        // The `ControllerConnection` class will handle remote controller connections on a different
        // thread.
        _baseServer.AddWebSocketService<ControllerConnection>(
            "/controllers",      // we expect connections on the /controllers path
            connection =>
            {
                // we use this initialization function to pass the message queue
                // to the connection worker and remember the id of the connection
                connection.Initialize(_nextNumericConnectionId++, _incomingConnectionUpdates);
                // ^ NOTICE: As far as I understand it, reference assignments as performed by this method  should be
                //           thread-safe. However, if strange things happen, we should look here first.
                Log("A new connection has been established.");
            });
        
        // Furthermore, we provide a websocket service offering diagnostic data about the game.
        _baseServer.AddWebSocketService<DiagnosticsConnection>("/diagnostics");

        // Now we need to get a handle to the session manager which
        // keeps track of all connections, so that we can use it later
        // to send data
        WebSocketServiceHost serviceHost;
        if (_baseServer.WebSocketServices.TryGetServiceHost("/controllers", out serviceHost))
        {
            _wssm = serviceHost.Sessions;
        }

        else
        {
            Log("Could not get a hold of the websocket service host. This means we can not send any messages.");
        }
        
        _baseServer.Start();
    }
    
    /**
     * Checks every frame, whether controller inputs or other updates regarding a controller connection arrived and
     * uses them to properly set up connections and relay inputs to `ControllerInput` instances.
     */
    void Update()
    {
        ConnectionUpdate updateBuffer; // buffer to store an inter-thread connection update message
        
        // For every update that has been received until this frame...
        while (_incomingConnectionUpdates.TryDequeue(out updateBuffer))
        {
            // Perform a different action depending on the type of connection update...
            updateBuffer.Match(new ConnectionUpdate.Matcher() {
                // If a controller sets its player name, this means the connection still needs to be set up
                NameUpdate = update => HandleConnectionSetup(update.name, update.connectionId, update.websocketId),
                
                // If a regular message arrived from a controller, forward it to the corresponding `ControllerInput` instance
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
                
                // If a controller connection disconnects / fails, reset the meta-data of this connection until we have
                // a new one and set the state of the `ControllerInput` instance to NotConnected.
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

    /**
     * Returns the controller input objects of all controllers which have connected so far.
     * Use the `OnNewController` event to get notified about new controllers.
     */
    public List<ControllerInput> GetInputs()
    {
        return _clients.GetInputs();
    }
    
    /**
     * `ControllerInput` class instances use this method to send messages to their controllers over the network.
     * You should NOT call this method yourself.
     *
     * @throws ApplicationException if currently there is no connection to the controller of that player.
     */
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

    /**
     * Internally used to send a message using a specific websocket.
     */
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
        //       often, however, SendTo is probably called only a few times per game session for every player to set up
        //       the connection. Most of the time we are only receiving data.
        //       If in the future, we want to send some data frequently, we could use a dedicated worker thread for this
        //       which ensures sending order by consuming a queue.
        //
        //       By the way, since WebSocket is implemented on top of TcpClient which is implemented on top of
        //       NetworkStream, reading and writing at the same time should be thread-safe.
        //       However, no two threads should send at the same time and no two threads should be receiving at the same
        //       time: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=netcore-3.1
    }
    
    /**
     * Sets up new controller connections or reconnects controllers which lost their connection.
     */
    private void HandleConnectionSetup(string name, int connectionId, string websocketId)
    {
        // check if there is already a player of this name...
        if (_clients.TryGetPlayerId(name, out var playerId))
        {
            if (_clients.TryGetInput(playerId, out var input))
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
                    SendToByWebsocketId(websocketId, new Message.NameOkMessage());
                    input.SetStatus(ConnectionStatus.Connected);
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
                    newPlayerId => new ControllerInput(newPlayerId, name, new PlayerInfo(), this)
                );
                newInput.SetStatus(ConnectionStatus.Connected);
                
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
     * Stops the websocket server when this game object is destroyed
     */
    private void OnDestroy()
    {
        _baseServer.Stop();
    }

    private void Log(String str) {
        Debug.Log("ControllerInput: " + str);
    }
    
    private void LogError(String str) {
        Debug.LogError("ControllerInput: " + str);
    }
}
