using System;
using System.Collections.Concurrent;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class Connection : WebSocketBehavior
{
    private ConcurrentQueue<Message> msgs;
    
    // to avoid allocation
    private Message messageBuffer = new Message();
    private ButtonMessage buttonMessageBuffer = new ButtonMessage();
    private JoystickMessage joystickMessage = new JoystickMessage();

    public void SetMessageQueue(ConcurrentQueue<Message> msgs)
    {
        this.msgs = msgs;
    }
    
    protected override void OnMessage(MessageEventArgs e)
    {
        Message msg = parseMessage(e.Data);
        if (msg != null)
        {
            msgs.Enqueue(msg);
        }
    }

    Message parseMessage(String str)
    {
        Debug.Log(str);
        
        JsonUtility.FromJsonOverwrite(str, messageBuffer);
        switch (messageBuffer.type)
        {
            case MessageType.Button:
                JsonUtility.FromJsonOverwrite(str, buttonMessageBuffer);
                return buttonMessageBuffer;

            case MessageType.Joystick:
                JsonUtility.FromJsonOverwrite(str, joystickMessage);
                return joystickMessage;
        }

        return null;
    }
}