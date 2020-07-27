using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

/**
 * Handles a single connection to a controller.
 *
 * It receives incoming messages, parses them from JSON strings into objects
 * and since it runs on a separate threat, it puts them into a message queue.
 *
 * For sending, see the methods of the `ControllerInput` class.
 */
public class ControllerConnection : WebSocketBehavior
{
    // Though the websocket library already provides an ID for every connection, we use our own numeric IDs since the
    // library uses string ids which are expensive to compare. And we need to compare IDs a LOT in the server update
    // function.
    // It is set by the server using the `Initialize` method of this class
    private int _connectionId = -1;
    
    // The message queue which is used to pass received messages and other events
    // to the Unity main thread.
    // This field will be set by the `Initialize` method, which is
    // called when a connection is instantiated.
    private ConcurrentQueue<ConnectionUpdate> _updatesToMainThread;
    
    // to avoid too much allocation for every received data, we create a buffer
    // into which the data will be parsed.
    private Message _messageBuffer = new Message(MessageType.None);

    /**
     * This method is called on initialization of a websocket connection in `ControllerServer`.
     * 
     * It passes us the numeric id this connection has been given and sets a reference to a queue which allows us to
     * send parsed messages to the main thread.
     */
    public void Initialize(
        int connectionId,
        ConcurrentQueue<ConnectionUpdate> updatesToMainThread
    )
    {
        this._connectionId = connectionId;
        this._updatesToMainThread = updatesToMainThread;
    }
    
    /**
     * Called every time a message is received.
     * It parses the message from JSON and puts the resulting object in
     * the queue buffer for the main thread.
     */
    protected override void OnMessage(MessageEventArgs e)
    {
        var msg = Message.FromJson(
            e.Data,
            _messageBuffer
        );
        
        if (msg != null)
        {
            // If the received message is the special "NameMessage" which is necessary to fully establish a connection
            // with a player name, also send a NameUpdate to the ControllerServer.
            // It can then handle the further steps to setup the connection.
            if (msg is Message.NameMessage message)
            {
                var name = message.name;
                
                _updatesToMainThread.Enqueue(
                    new ConnectionUpdate.NameUpdate(_connectionId, name, ID)
                );
            }
            
            // For all messages, simply forward them to `ControllerServer` with the annotated connection id
            _updatesToMainThread.Enqueue(
                new ConnectionUpdate.MessageUpdate(_connectionId, msg)
            );
        }
    }

    protected override void OnClose(CloseEventArgs e)
    {
        _updatesToMainThread.Enqueue(
            new ConnectionUpdate.DisconnectUpdate(_connectionId)
        );
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError($"Connection: Error in connection to controller: {e.Message}");
    }
}
