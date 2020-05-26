using System;
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
public class Connection : WebSocketBehavior
{
    // The message queue which is used to pass received messages to the
    // Unity main thread.
    // This field will be set by the `SetMessageQueue` method, which is
    // called when a connection is instantiated.
    private ConcurrentQueue<Message> _msgsToMainThread;
    
    // to avoid too much allocation for every received data, we create a buffer
    // into which the data will be parsed.
    private Message _messageBuffer = new Message();

    /**
     * This method is called on initialization to set the reference to
     * a queue which allows us to send parsed messages to the main thread.
     */
    public void SetMessageQueue(ConcurrentQueue<Message> msgs)
    {
        this._msgsToMainThread = msgs;
    }
    
    /**
     * Called every time a message is received.
     * It parses the message from JSON and puts the resulting object in
     * the queue buffer for the main thread.
     */
    protected override void OnMessage(MessageEventArgs e)
    {
        Message msg = Message.FromJson(
            e.Data,
            _messageBuffer
        );
        
        if (msg != null)
        {
            _msgsToMainThread.Enqueue(msg);
        }
    }
}
