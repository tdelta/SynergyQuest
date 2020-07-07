using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
 * `ControllerInput` instance emitted by this event. See also the `Input` interface
 * which is implemented by this class.
 * That's it :).
 * 
 * You can then for example check whether the `Attack` button is pressed like
 * this:
 * `controllerInput.GetButton(Button.Attack)`
 *
 * And you can retrieve the values of the horizontal or vertical joystick
 * axes like this:
 * `controllerInput.GetVertical()`
 *
 * You can assign a player color like this:
 * `controllerInputCo.SetColor(PlayerColor.Red);`
 *
 * `ControllerInput` allows to access inputs pressed on the controller without having to
 * know about the low-level details on how the connection is managed.
 * There is one exception to this: A controller might at some point in time lose its connection to the game. If this
 * happens, the `OnDisconnect` event is emitted and the game should be paused until the `OnReconnect` event is emitted.
 * 
 * See also `ControllerDebugUISpawner` and `ControllerDebugUI` for a more complete usage example.
 */
public class ControllerInput: Input
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

    // We cache the last inputs reported by a controller here
    private float _vertical   = 0.0f;  // vertical joystick position
    private float _horizontal = 0.0f;  // horizontal joystick position

    // TODO: Maybe use an automatically created dictionary of ButtonPressState, so we dont need to repeat code
    private ButtonPressState _attackButtonState = new ButtonPressState(); // whether the Attack button is pressed
    private ButtonPressState _pullButtonState = new ButtonPressState(); // whether the Pull button is pressed
    private ButtonPressState _pressButtonState = new ButtonPressState(); // whether the Press button is pressed
 
    // We also cache the last special values set by the game
    // FIXME: We must resend this stuff to the client on reconnect if the client temporarily disconnects.
    //        We should also cache and resend the menu actions
    private PlayerColor _playerColor = PlayerColor.Any;
    private GameState _gameState = GameState.Lobby;

    /**
     * This event is emitted when the underlying controller loses its connection to the game. The game should be paused
     * when this is emitted until `OnReconnect` is emitted.
     */
    public event DisconnectAction OnDisconnect;
    public delegate void DisconnectAction(ControllerInput input);

    /**
     * This event is emitted when a certain menu action is selected on the controller.
     */
    public event MenuActionTriggeredAction OnMenuActionTriggered;
    
    /**
     * This event is emitted when the underlying controller reconnects to the game after the connection has been lost
     * temporarily. The game should be paused when `OnDisconnect` is emitted and resumed when `OnReconnect` is emitted.
     */
    public event ReconnectAction OnReconnect;
    public delegate void ReconnectAction(ControllerInput input);
    
    /**
     * Returns the value of the Joystick position on the vertical axis.
     * 
     * The value is in [-1; 1], at least clients are required to only send
     * such values.
     */
    public float GetVertical()
    {
        return _vertical;
    }

    /**
     * Returns the value of the Joystick position on the horizontal axis.
     * 
     * The value is in [-1; 1], at least clients are required to only send
     * such values.
     */
    public float GetHorizontal()
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
                return _attackButtonState.GetValue();
            case Button.Pull:
                return _pullButtonState.GetValue();
            case Button.Press:
                return _pressButtonState.GetValue();
        }

        return false;
    }
    
    /**
     * Returns whether a specific button has been pressed during the current frame.
     * It will not return true during the next frames until the button has been pressed again.
     * 
     * For the different button ids, see the `Button` enum.
     */
    public bool GetButtonDown(Button b)
    {
        switch (b)
        {
            case Button.Attack:
                return _attackButtonState.GetValueDown();
            case Button.Pull:
                return _pullButtonState.GetValueDown();
            case Button.Press:
                return _pressButtonState.GetValueDown();
        }

        return false;
    }
    
    /**
     * Returns whether a specific button has been released during the current frame.
     * It will not return true during the next frames until the button has been pressed and released again.
     * 
     * For the different button ids, see the `Button` enum.
     */
    public bool GetButtonUp(Button b)
    {
        switch (b)
        {
            case Button.Attack:
                return _attackButtonState.GetValueUp();
            case Button.Pull:
                return _pullButtonState.GetValueUp();
            case Button.Press:
                return _pressButtonState.GetValueUp();
        }

        return false;
    }

    /**
     * Tells the controlled that an action can or cannot be performed
     * (such as reading a sign or pressing a button)
     * 
     * @throws ApplicationError if the controller is currently not connected
     */
    public void SetGameAction(Button action, bool enabled)
    {
        var msg = new Message.SetGameActionMessage(action, enabled);
        SendMessage(msg);
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

    /**
     * Retrieve the color that has been assigned to this controller.
     */
    public PlayerColor GetColor()
    {
        return _playerColor;
    }

    /**
     * Tell the controller to vibrate. This will only have an effect if the controller
     * supports vibration.
     * 
     * @param vibrationPattern Indicates how the controller shall vibrate.
     *   The first number is the number of milliseconds to vibrate,
     *   the next is the number to milliseconds to pause,
     *   the number after that is again a number of milliseconds to vibrate and so on.
     *
     *   Hence these are numbers of milliseconds to vibrate and pause in
     *   alteration.
     */
    public void PlayVibrationFeedback(List<float> vibrationPattern)
    {
        var msg = new Message.VibrationSequenceMessage(vibrationPattern);
        SendMessage(msg);
    }

    /**
     * DO NOT USE OUTSIDE OF ControllerInput LIBRARY.
     * USE `SharedControllerState.Instance.EnableMenuActions` INSTEAD.
     * 
     * Tells the controller, which menu actions are currently available in the game.
     * For example, when the game can be paused, MenuAction.PauseGame should be enabled.
     *
     * @throws ApplicationError if the controller is currently not connected
     */
    public void SetEnabledMenuActions(HashSet<MenuAction> enabledActions)
    {
        var msg = new Message.SetMenuActionsMessage(
            enabledActions.ToList()
        );
        
        SendMessage(msg);
    }
    
    /**
     * DO NOT USE OUTSIDE OF ControllerInput LIBRARY.
     * USE `SharedControllerState.Instance.SetGameState` INSTEAD.
     * 
     * Tells the controller, in which state the game currently is.
     * For example, if the game is displaying the lobby and then starts, it should tell the controllers here
     * that the game is now in the state `GameState.Started`.
     *
     * @throws ApplicationError if the controller is currently not connected
     */
    public void SetGameState(GameState state)
    {
        _gameState = state;
        
        var msg = new Message.GameStateChangedMessage(state);
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
                OnReconnect?.Invoke(this);
                break;
            
            case ConnectionStatus.NotConnected:
                if (previousStatus == ConnectionStatus.Connected)
                {
                    OnDisconnect?.Invoke(this);
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
                        _attackButtonState.ProcessRawInput(msg.onOff);
                        break;
                    case Button.Pull:
                        _pullButtonState.ProcessRawInput(msg.onOff);
                        break;
                    case Button.Press:
                        _pressButtonState.ProcessRawInput(msg.onOff);
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
                throw new ApplicationException("Can not send message if controller is not connected.");
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

enum ButtonValue
{
    Inactive,
    ButtonDown,
    ButtonHeld,
    ButtonUp
}

class ButtonPressState
{
    private ButtonValue _value = ButtonValue.Inactive;
    private int _lastRecalculation = -1;

    private void CheckForFrameUpdate()
    {
        // Only change something, if the current frame changed
        if (_lastRecalculation != Time.frameCount)
        {
            switch (_value)
            {
                case ButtonValue.ButtonDown:
                    _value = ButtonValue.ButtonHeld;
                    break;
                case ButtonValue.ButtonUp:
                    _value = ButtonValue.Inactive;
                    break;
            }
            
            _lastRecalculation = Time.frameCount;
        }
    }

    public bool GetValue()
    {
        CheckForFrameUpdate();

        return _value == ButtonValue.ButtonDown || _value == ButtonValue.ButtonHeld;
    }

    public bool GetValueDown()
    {
        CheckForFrameUpdate();

        return _value == ButtonValue.ButtonDown;
    }

    public bool GetValueUp()
    {
        CheckForFrameUpdate();

        return _value == ButtonValue.ButtonUp;
    }
    
    public void ProcessRawInput(bool rawInputOnOff)
    {
        CheckForFrameUpdate();
        
        if (rawInputOnOff)
        {
            switch (_value)
            {
                case ButtonValue.Inactive:
                    _value = ButtonValue.ButtonDown;
                    break;
                case ButtonValue.ButtonUp:
                    _value = ButtonValue.ButtonDown;
                    break;
            }
        }

        else
        {
            switch (_value)
            {
                case ButtonValue.ButtonDown:
                    _value = ButtonValue.ButtonUp;
                    break;
                case ButtonValue.ButtonHeld:
                    _value = ButtonValue.ButtonUp;
                    break;
            }
        }
    }
}
