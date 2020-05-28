using System;
using System.Collections.Concurrent;
using Boo.Lang.Runtime;
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
 * `controllerInput.SetColor("#c0ffee");`
 *
 * Internally, this class uses a websocket connection to the controller over
 * which JSON messages are exchanged. For the format of the JSON messages,
 * see the `Message` class hierarchy.
 * Since the native C# WebSocket functionalities which are required for
 * servers (a compatible HTTPListener implementation) do not ship with .NET
 * runtime of the current version of Unity (at least not on Linux), we use
 * the MIT licensed library websocket-sharp for this.
 */
public class ControllerInput : MonoBehaviour
{
    // Accepts websocket connections
    private WebSocketServer _wss;
    // Keeps track of all connections
    private WebSocketSessionManager _wssm;
    
    // Since the websocket server runs on its own thread, it acts like a
    // producer which puts received messages in this thread-safe queue.
    //
    // Then, the main thread of Unity acts as a consumer in the `Update`
    // function.
    private ConcurrentQueue<Message> _incomingMsgs = new ConcurrentQueue<Message>();

    // Right now, we have only one player and few inputs, so we keep track
    // of their state right here for now.
    private Connection _connection;    // websocket connection to the client. Needed for sending data to a specific connection
    private float _vertical = 0.0f;    // vertical joystick position
    private float _horizontal = 0.0f;  // horizontal joystick position
    private bool _attack_down = false; // whether the Attack button is pressed
    private bool _pull_down = false;   // whether the Pull button is pressed
    
    // Port the game will listen on for connections from controllers
    private const short _port = 4242;
    
    /**
     * Starts listening for websocket connections of controllers, as soon as
     * this game object is enabled.
     */
    void OnEnable()
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
                connection.SetMessageQueue(_incomingMsgs);
                Log("Got id: " + connection);
                this._connection = connection;
                // ^ NOTICE: As far as I understand it, reference assignment should be
                //           thread-safe. However, if strange things happen, we should look
                //           here first.
                Log("A new controller connection has been established.");
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

    /**
     * Checks every frame, whether inputs arrived and remembers them in case
     * some game object asks for them.
     */
    void Update()
    {
        Message msg; // buffer to store a message
        
        // For every message that has been received until this frame
        while (_incomingMsgs.TryDequeue(out msg))
        {
            // determine what kind of message it is and change the state
            // accordingly
            switch (msg.type)
            {
                case MessageType.Button:
                    var buttonMsg = (ButtonMessage) msg;
                    switch (buttonMsg.button)
                    {
                        case Button.Attack:
                            _attack_down = buttonMsg.onOff;
                            break;
                        case Button.Pull:
                            _pull_down = buttonMsg.onOff;
                            break;
                    }
                    break;
                case MessageType.Joystick:
                    var joystickMsg = (JoystickMessage) msg;
                    _vertical = joystickMsg.vertical;
                    _horizontal = joystickMsg.horizontal;
                    break;
            }
        }
    }

    /**
     * When disabling the game object, stop receiving messages and clear
     * all buffered messages.
     */
    void OnDisable()
    {
        _wss.Stop();
        ClearMsgQueue();
    }

    /**
     * Returns the value of the Joystick position on the vertical axis.
     * 
     * The value is in [-1; 1], at least clients are required to only send
     * such values.
     */
    public float Vertical()
    {
        return _vertical;
    }

    /**
     * Returns the value of the Joystick position on the horizontal axis.
     * 
     * The value is in [-1; 1], at least clients are required to only send
     * such values.
     */
    public float Horizontal()
    {
        return _horizontal;
    }

    /**
     * Returns whether a specific button is currently pressed or not.
     * For the different button ids, see the `Button` enum.
     */
    public bool GetButton(Button b)
    {
        switch (b)
        {
            case Button.Attack:
                return _attack_down;
            case Button.Pull:
                return _pull_down;
        }

        return false;
    }

    public void SetColor(string color)
    {
        if (_connection != null)
        {
            var msg = new PlayerColorMessage(color);
            
            this._wssm.SendTo(
                msg.ToJson(), _connection.ID
            ); // synchronous sending! See also note below 
        }

        else
        {
            throw new RuntimeException("Can not set color if no controller is connected.");
        }

        // NOTE: There is also a method SendToAsync, however, if I understand the source code of websocket-sharp
        //       correctly, while there is a lock for thread-safety for sending in the WebSocket implementation, there
        //       is no guarantee of order for the Async method.
        //       (Since the asynchronicity is implemented on top of BeginInvoke:
        //        https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/calling-synchronous-methods-asynchronously
        //       )
        //       Hence, we use the synchronous send method here. This could result in a drop of frame rate when called
        //       often, however, SetColor is probably called only once per game session for every player.
        //       If in the future, we want to send some data frequently, we could use a dedicated worker thread for this
        //       which ensures sending order by consuming a queue.
        //
        //       By the way, since WebSocket is implemented on top of TcpClient which is implemented on top of
        //       NetworkStream, reading and writing at the same time should be thread-safe.
        //       However, no two threads should send at the same time and no two threads should be receiving at the same
        //       time: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream?view=netcore-3.1
    }
    
    private void Log(String str) {
        Debug.Log("ControllerInput: " + str);
    }

    /**
     * Removes all remaining received messages from the buffer and throws
     * them away.
     */
    private void ClearMsgQueue()
    {
        Message msg;
        while (!_incomingMsgs.IsEmpty)
        {
            _incomingMsgs.TryDequeue(out msg);
        }
    }
}
