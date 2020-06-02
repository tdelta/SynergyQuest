using System;
using System.Security.Permissions;
using UnityEngine;

/**
 * Ids for the different kinds of messages which can be sent by the controller.
 * See the `Message` class hierarchy.
 */
public enum MessageType
{
    Button = 0,            // sent by controller
    Joystick = 1,          // sent by controller
    PlayerColor = 2,       // sent by game
    Name = 3,              // sent by controller
    NameTaken = 4,         // sent by game
    NameOk = 5,            // sent by game
    MaxPlayersReached = 6, // sent by game
    None = 7               // placeholder type for base class
}

/**
 * The classes of this hierarchy represent the JSON data that is communicated
 * between the controllers and the game.
 *
 * The `Message` base class only carries a `type` identifier which is used
 * during deserialization, which message has been send.
 * See also the sub-classes.
 */
[Serializable]
public class Message
{
    public MessageType type;

    public Message(MessageType type)
    {
        this.type = type;
    }

    /**
     * Parses a message from a JSON string.
     * 
     * To reduce allocations a bit, at least a buffers for the base message
     * type is required as parameter, so that message objects can be
     * constructed in-place in memory.
     */
    public static Message FromJson(String str, Message messageBuffer) {
        // First parse the base class.
        // For the base class we can parse in-place in a buffer, since
        // it won't be passed on to the main thread.
        JsonUtility.FromJsonOverwrite(str, messageBuffer);
        
        // Now we can retrieve the type to parse a more accurate sub-class.
        // This approach might seem weird, but actually the Unity docs do
        // recommend to do it like this:
        // https://docs.unity3d.com/Manual/JSONSerialization.html
        switch (messageBuffer.type)
        {
            case MessageType.Button:
                return JsonUtility.FromJson<ButtonMessage>(str);

            case MessageType.Joystick:
                return JsonUtility.FromJson<JoystickMessage>(str);
            
            case MessageType.PlayerColor:
                return JsonUtility.FromJson<PlayerColorMessage>(str);
            
            case MessageType.Name:
                return JsonUtility.FromJson<NameMessage>(str);
            
            case MessageType.NameTaken:
                return JsonUtility.FromJson<NameTakenMessage>(str);
            
            case MessageType.NameOk:
                return JsonUtility.FromJson<NameOkMessage>(str);
            
            case MessageType.MaxPlayersReached:
                return JsonUtility.FromJson<MaxPlayersReachedMessage>(str);
        }

        return null;
    }

    /**
     * Serialize this message into JSON. Since the JsonUtility class we use for this seems to handle the polymorphism
     * of this Message type on its own, we need no overrides.
     */
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public class Matcher
    {
        public Action<ButtonMessage> ButtonMessage = _ => { };
        public Action<JoystickMessage> JoystickMessage = _ => { };
        public Action<PlayerColorMessage> PlayerColorMessage = _ => { };
        public Action<NameMessage> NameMessage = _ => {};
        public Action<NameTakenMessage> NameTakenMessage = _ => {};
        public Action<NameOkMessage> NameOkMessage = _ => {};
        public Action<MaxPlayersReachedMessage> MaxPlayersReachedMessage = _ => {};
    }

    public virtual void Match(Matcher matcher)
    {
        throw new ApplicationException("You can not match on the message base class.");
    }

    [Serializable]
    public sealed class ButtonMessage : Message
    {
        public Button button;
        public bool onOff;
        
        public ButtonMessage(Button button, bool onOff)
            : base(MessageType.Button)
        {
            this.button = button;
            this.onOff = onOff;
        }

        public override void Match( Matcher matcher )
        {
            matcher.ButtonMessage(this);
        }
    }

    [Serializable]
    public sealed class JoystickMessage : Message
    {
        public float vertical;
        public float horizontal;
        
        public JoystickMessage(float vertical, float horizontal)
            : base(MessageType.Joystick)
        {
            this.vertical = vertical;
            this.horizontal = horizontal;
        }
        
        public override void Match( Matcher matcher )
        {
            matcher.JoystickMessage(this);
        }
    }

    [Serializable]
    public sealed class PlayerColorMessage : Message
    {
        // Hexadecimal color value with leading '#'
        public string color;

        public PlayerColorMessage(string color)
            : base(MessageType.PlayerColor)
        {
            this.color = color;
        }
        
        public override void Match( Matcher matcher )
        {
            matcher.PlayerColorMessage(this);
        }
    }

    [Serializable]
    public sealed class NameMessage : Message
    {
        // Human-readable name
        public string name;

        public NameMessage(string name)
            : base(MessageType.Name)
        {
            this.name = name;
        }
        
        public override void Match( Matcher matcher )
        {
            matcher.NameMessage(this);
        }
    }

    [Serializable]
    public sealed class NameTakenMessage : Message
    {
        // Human-readable name
        public string name;

        public NameTakenMessage(string name)
            : base(MessageType.NameTaken)
        {
            this.name = name;
        }
        
        public override void Match( Matcher matcher )
        {
            matcher.NameTakenMessage(this);
        }
    }

    [Serializable]
    public sealed class NameOkMessage : Message
    {
        public NameOkMessage()
            : base(MessageType.NameOk)
        { }
        
        public override void Match( Matcher matcher )
        {
            matcher.NameOkMessage(this);
        }
    }
    
    [Serializable]
    public sealed class MaxPlayersReachedMessage : Message
    {
        public MaxPlayersReachedMessage()
            : base(MessageType.MaxPlayersReached)
        { }
        
        public override void Match( Matcher matcher )
        {
            matcher.MaxPlayersReachedMessage(this);
        }
    }
}

