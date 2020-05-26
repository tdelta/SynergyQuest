using System;
using System.Collections.Concurrent;
using UnityEngine;

using WebSocketSharp.Server;

public class ControllerInput : MonoBehaviour
{
    private WebSocketServer wss;
    private ConcurrentQueue<Message> msgs = new ConcurrentQueue<Message>();

    private float _vertical = 0.0f;
    private float _horizontal = 0.0f;
    private bool _attack_down = false;
    private bool _pull_down = false;
    
    void Log(String str) {
        Debug.Log("ControllerInput: " + str);
    }

    void ClearMsgQueue()
    {
        Message msg;
        while (!msgs.IsEmpty)
        {
            msgs.TryDequeue(out msg);
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        Log("Starting HTTP server on port 4242...");
        
        wss = new WebSocketServer("ws://127.0.0.1:4242");
        wss.AddWebSocketService<Connection>(
            "/sockets",
            connection => connection.SetMessageQueue(msgs)
        );
        wss.Start();
    }

    void Update()
    {
        Message msg;
        while (msgs.TryDequeue(out msg))
        {
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

    void OnDisable()
    {
        wss.Stop();
        ClearMsgQueue();
    }

    public float Vertical()
    {
        return _vertical;
    }

    public float Horizontal()
    {
        return _horizontal;
    }

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
}
