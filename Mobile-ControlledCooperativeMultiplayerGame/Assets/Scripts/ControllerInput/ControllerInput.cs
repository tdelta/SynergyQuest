using System;

/**
 * While the `ControllerServer` class accepts and manages websocket connections,
 * it produces instances of this class for every fully connected controller.
 * 
 * This class allows to receive inputs from remote controllers over the network
 * and also share some data with the controllers (e.g. assign them a color).
 * Its usage is similar to the `Input` class of Unity, though much simpler.
 *
 * To use this class, listen to the `OnNewController` event of a game object
 * implementing the `ControllerServer` component.
 * You can then give your character control script a reference to the
 * `ControllerInput` instance emitted by this event.
 * That's it :).
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
 * `ControllerInput` allows to access inputs pressed on the controller without having to
 * know about the low-level details on how the connection is managed.
 * There is one exception to this: A controller might at some point in time lose its connection to the game. If this
 * happens, the `OnDisconnect` event is emitted and the game should be paused until the `OnReconnect` event is emitted.
 * 
 * See also `ControllerDebugUISpawner` and `ControllerDebugUI` for a more complete usage example.
 */
public class ControllerInput
{
    // Every player gets assigned an ID. Their controller is assigned the same ID which is stored here so that this
    // class may communicate with `ControllerServer` to send messages to the specific controller controller associated
    // with this player
    // `ControllerServer` is used for the underlying network communication
    private readonly ControllerServer _controllerServer;
    private ConnectionStatus _connectionStatus = ConnectionStatus.NotConnected;

    public ConnectionStatus ConnectionStatus => _connectionStatus;

    public int PlayerId { get; }
    public string PlayerName { get; }
    public PlayerColor Color => _playerColor;

    // We cache the last inputs reported by a controller here
    private float _vertical   = 0.0f;  // vertical joystick position
    private float _horizontal = 0.0f;  // horizontal joystick position
    private bool _attackDown  = false; // whether the Attack button is pressed
    private bool _pullDown    = false; // whether the Pull button is pressed
    
    // We also cache the last special values set by the game
    // FIXME: We must resend this stuff to the client on reconnect if the client temporarily disconnects.
    //        We should also cache and resend the menu actions
    private PlayerColor _playerColor;

    /**
     * This event is emitted when the underlying controller loses its connection to the game. The game should be paused
     * when this is emitted until `OnReconnect` is emitted.
     */
    public event DisconnectAction OnDisconnect;
    public delegate void DisconnectAction();

    /**
     * This event is emitted when a certain menu action is selected on the controller.
     */
    public event MenuActionTriggeredAction OnMenuActionTriggered;
    public delegate void MenuActionTriggeredAction(MenuAction action);
    
    /**
     * This event is emitted when the underlying controller reconnects to the game after the connection has been lost
     * temporarily. The game should be paused when `OnDisconnect` is emitted and resumed when `OnReconnect` is emitted.
     */
    public event ReconnectAction OnReconnect;
    public delegate void ReconnectAction();
    
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
                return _attackDown;
            case Button.Pull:
                return _pullDown;
        }

        return false;
    }

    /**
     * Tells the controller, which color has been assigned to its player.
     *
     * @throws ApplicationError if the controller is currently not connected
     */
    public void SetColor(PlayerColor color)
    {
        _playerColor = color;
        
        var msg = new Message.PlayerColorMessage(color);
        SendMessage(msg);
    }

    public void EnableMenuAction(MenuAction action, bool enable = true)
    {
        var msg = new Message.SetMenuActionMessage(
            action,
            enable
        );
        
        SendMessage(msg);
    }
    
    /**
     * Creates an instance of this class. **This should only be called by `ControllerServer` internally.**
     * 
     * Use the `OnNewController` event of the `ControllerServer` class instead to gain a `ControllerInput` instance.
     */
    public ControllerInput(int playerId, string playerName, ControllerServer server)
    {
        this.PlayerId = playerId;
        this.PlayerName = playerName;
        this._controllerServer = server;
    }
    
    /**
     * `ControllerServer` calls this method to inform this class, whether the underlying network connection is currently
     * established or not. Do NOT call it yourself.
     */
    public void SetStatus(ConnectionStatus status)
    {
        var previousStatus = _connectionStatus;
        _connectionStatus = status;

        switch (status)
        {
            case ConnectionStatus.Connected:
                // FIXME: Cache whole state and send the state on reconnect. This way, activated menu actions stay active etc.
                OnReconnect?.Invoke();
                break;
            
            case ConnectionStatus.NotConnected:
                if (previousStatus == ConnectionStatus.Connected)
                {
                    OnDisconnect?.Invoke();
                }
                break;
        }
    }
    
    /**
     * Allows to check whether the controller underlying this input is currently checked.
     * You should usually NOT use this method, use the `OnDisconnect` / `OnReconnect` events instead.
     *
     * This method is internally used by `ControllerServer`.
     */
    public bool IsConnected()
    {
        return _connectionStatus is ConnectionStatus.Connected;
    }
    
    /**
     * This method is called by `ControllerServer` to relay network messages of the controller to this class.
     * You should NOT call it yourself.
     *
     * It caches the inputs received from a controller.
     */
    public void HandleMessage(Message baseMsg)
    {
        // determine what kind of message it is and change the state
        // accordingly
        baseMsg.Match(new Message.Matcher()
        {
            ButtonMessage = msg =>
            {
                switch (msg.button)
                {
                    case Button.Attack:
                        _attackDown = msg.onOff;
                        break;
                    case Button.Pull:
                        _pullDown = msg.onOff;
                        break;
                }
            },
            
            JoystickMessage = msg =>
            {
                _vertical = msg.vertical;
                _horizontal = msg.horizontal;
            },
            
            MenuActionTriggeredMessage = msg =>
            {
                OnMenuActionTriggered?.Invoke(msg.menuAction);
            }
        });
    }

    /**
     * Sends a network message to the underlying controller.
     *
     * @throws ApplicationException if the controller is currently not connected
     */
    private void SendMessage(Message msg)
    {
        switch (_connectionStatus)
        {
            case ConnectionStatus.Connected:
                _controllerServer.SendTo(PlayerId, msg);
                break;
            
            default:
                throw new ApplicationException("Can not set color if no controller is connected.");
        }
    }
}

/**
 * Documents the current state of the connection of a controller to the game.
 */
public enum ConnectionStatus
{
    NotConnected, // the controller has disconnected
    Connected     // the controller is connected
}
