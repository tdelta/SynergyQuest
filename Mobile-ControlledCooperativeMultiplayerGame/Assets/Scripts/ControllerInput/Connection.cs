using System;
using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public abstract class ConnectionUpdate
{
    public readonly int connectionId;

    public sealed class NameUpdate : ConnectionUpdate
    {
        public readonly string name;
        public readonly string websocketId;

        public NameUpdate(int connectionId, string name, string websocketId)
            : base(connectionId)
        {
            this.name = name;
            this.websocketId = websocketId;
        }
        
        public override void Match( Matcher matcher )
        {
            matcher.NameUpdate(this);
        }
    }
    
    public sealed class MessageUpdate : ConnectionUpdate
    {
        public readonly Message message;

        public MessageUpdate(int connectionId, Message msg)
            : base(connectionId)
        {
            this.message = msg;
        }

        public override void Match( Matcher matcher )
        {
            matcher.MessageUpdate(this);
        }
    }

    public sealed class DisconnectUpdate : ConnectionUpdate
    {
        public DisconnectUpdate(int connectionId)
            : base(connectionId)
        { }
        
        public override void Match( Matcher matcher )
        {
            matcher.DisconnectUpdate(this);
        }
    }

    public sealed class Matcher
    {
        public Action<NameUpdate> NameUpdate = _ => {};
        public Action<MessageUpdate> MessageUpdate = _ => { };
        public Action<DisconnectUpdate> DisconnectUpdate = _ => {};
    }

    public abstract void Match( Matcher matcher );

    private ConnectionUpdate(int connectionId)
    {
        this.connectionId = connectionId;
    }
}


/**
 * Handles a single connection to a controller.
 *
 * It receives incoming messages, parses them from JSON strings into objects
 * and since it runs on a separate threat, it puts them into a message queue.
 *
 * For sending, see the methods of the `ControllerInput` class.
 */
public class Connection : WebSocketBehavior
{
    // Though the websocket library already provides an ID for every connection, we use our own numeric IDs since th
    // library uses string ids which are expensive to compare. And we need to compare IDs a LOT in the server update
    // function.
    // It is set by the server using the `Initialize` method of this class
    private int _connectionId = -1;
    
    // The message queue which is used to pass received messages to the
    // Unity main thread.
    // This field will be set by the `SetMessageQueue` method, which is
    // called when a connection is instantiated.
    private ConcurrentQueue<ConnectionUpdate> _updatesToMainThread;
    
    // to avoid too much allocation for every received data, we create a buffer
    // into which the data will be parsed.
    private Message _messageBuffer = new Message(MessageType.None);

    /**
     * This method is called on initialization to set the reference to
     * a queue which allows us to send parsed messages to the main thread.
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
            if (msg is Message.NameMessage message)
            {
                var name = message.name;
                
                _updatesToMainThread.Enqueue(
                    new ConnectionUpdate.NameUpdate(_connectionId, name, ID)
                );
            }
            
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
