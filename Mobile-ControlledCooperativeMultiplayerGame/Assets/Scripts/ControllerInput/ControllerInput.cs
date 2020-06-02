using System;
using UnityEngine;

/**
 * Simulated ADT representing the current status of the connection to a controller.
 * (If we get tired of simulating ADTs in C#, I maybe could use F# instead: https://www.davideaversa.it/blog/quick-look-at-f-in-unity/)
 *
 * FIXME: Transfer this description
 */
public enum ConnectionStatus
{
    NotConnected,
    Connected
}

public class ControllerInput
{
    private int _playerId;
    private ControllerServer _controllerServer;
    private ConnectionStatus _connectionStatus = ConnectionStatus.NotConnected;

    public ConnectionStatus ConnectionStatus => _connectionStatus;

    public int PlayerId => _playerId;

    private float _vertical = 0.0f;    // vertical joystick position
    private float _horizontal = 0.0f;  // horizontal joystick position
    private bool _attackDown = false; // whether the Attack button is pressed
    private bool _pullDown = false;   // whether the Pull button is pressed

    public delegate void ReconnectAction();
    public delegate void DisconnectAction();

    public event ReconnectAction OnReconnect;
    public event DisconnectAction OnDisconnect;
    
    // FIXME: Find a way to only let the server access this method
    public void SetStatus(ConnectionStatus status)
    {
        var previousStatus = _connectionStatus;
        _connectionStatus = status;

        switch (status)
        {
            case ConnectionStatus.Connected:
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

    public ControllerInput(int playerId, ControllerServer server)
    {
        this._playerId = playerId;
        this._controllerServer = server;
    }
    
    public bool IsConnected()
    {
        return _connectionStatus is ConnectionStatus.Connected;
    }

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
            }
        });
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
                return _attackDown;
            case Button.Pull:
                return _pullDown;
        }

        return false;
    }

    public void SetColor(string color)
    {
        var msg = new Message.PlayerColorMessage(color);
        
        SendMessage(msg);
    }

    private void SendMessage(Message msg)
    {
        switch (_connectionStatus)
        {
            case ConnectionStatus.Connected:
                _controllerServer.SendTo(_playerId, msg);
                break;
            
            default:
                throw new ApplicationException("Can not set color if no controller is connected.");
        }
    }
}
