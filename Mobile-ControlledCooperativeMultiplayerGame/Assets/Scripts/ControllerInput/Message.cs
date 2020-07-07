using System;
using UnityEngine;

/**
 * This file describes the format of messages sent between controller and
 * game and provides utilities to create, (de-)serialize and handle messages.
 *
 * It is only used internally by the classes of this subfolder and is not intended to be
 * used directly.
 *
 * Similar code can be found in the controller client library in the file `Message.ts`
 */

/**
 * Ids for the different kinds of messages which can be sent by the controller.
 * See the `Message` class hierarchy.
 */
public enum MessageType
{
    None = 0,                // Placeholder type for base class
    Name = 1,                // Name of the player, sent by controller upon establishing a connection
    NameTaken = 2,           // Game rejects player name since it is already used by another controller, sent by game
    NameOk = 3,              // Game accepts player name, sent by game
    MaxPlayersReached = 4,   // Game rejects connection since the maximum number of controllers is already connected to it, sent by game
    Button = 5,              // Button input, sent by controller
    Joystick = 6,            // Joystick input, sent by controller
    PlayerColor = 7,         // Color assigned to a player, sent by the game
    SetMenuAction = 8,       // Enable / disable a menu action, sent by game
    MenuActionTriggered = 9, // A menu action has been selected on the controller
    GameStateChanged = 10,   // The state of the game changed, e.g. Lobby -> Game started. Sent by the game
    SetGameAction = 11           // Enable / disable a button in the controller UI, sent by game
}

/**
 * The classes of this hierarchy represent the JSON data that is communicated
 * between the controllers and the game.
 *
 * The `Message` base class only carries a `type` identifier which is used
 * during deserialization, to determine which message has been send.
 * All concrete kinds of messages are subclasses of `Message`.
 * 
 * One can do a "switch-case" on them using the `Match` method. See the `ControllerInput` class for an example
 * Hence, the Message Types implementation simulates an ADT.
 * ADTs are a concept known from functional programming languages like Haskell or object-oriented
 * languages with integration of functional concepts (Kotlin, Scala).
 * If we get tired of simulating ADTs in C#, we maybe could use F# instead: https://www.davideaversa.it/blog/quick-look-at-f-in-unity/
 *
 * The point is that ADTs are well suited for modelling different kinds of messages.
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
            
            case MessageType.SetMenuAction:
                return JsonUtility.FromJson<SetMenuActionMessage>(str);
            
            case MessageType.MenuActionTriggered:
                return JsonUtility.FromJson<MenuActionTriggeredMessage>(str);

            case MessageType.SetGameAction:
                return JsonUtility.FromJson<SetGameActionMessage>(str);
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

        /**
         * See base class method for an explanation.
         */
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
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.JoystickMessage(this);
        }
    }

    [Serializable]
    public sealed class PlayerColorMessage : Message
    {
        public PlayerColor color;

        public PlayerColorMessage(PlayerColor color)
            : base(MessageType.PlayerColor)
        {
            this.color = color;
        }
        
        /**
         * See base class method for an explanation.
         */
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
        
        /**
         * See base class method for an explanation.
         */
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
        
        /**
         * See base class method for an explanation.
         */
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
        
        /**
         * See base class method for an explanation.
         */
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
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.MaxPlayersReachedMessage(this);
        }
    }
   
    [Serializable]
    public sealed class SetGameActionMessage : Message
    {
        public Button button;
        public bool enabled;

        public SetGameActionMessage(Button button, bool enabled)
            : base(MessageType.SetGameAction)
        {
            this.button = button;
            this.enabled = enabled;
        }
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.SetGameActionMessage(this);
        }
    }

    [Serializable]
    public sealed class SetMenuActionMessage : Message
    {
        public MenuAction menuAction;
        public bool enabled;
        
        public SetMenuActionMessage(MenuAction action, bool enabled)
            : base(MessageType.SetMenuAction)
        {
            this.menuAction = action;
            this.enabled = enabled;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.SetMenuActionMessage(this);
        }
    }
    
    [Serializable]
    public sealed class MenuActionTriggeredMessage : Message
    {
        public MenuAction menuAction;
        
        public MenuActionTriggeredMessage(MenuAction action)
            : base(MessageType.MenuActionTriggered)
        {
            this.menuAction = action;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.MenuActionTriggeredMessage(this);
        }
    }
    
    [Serializable]
    public sealed class GameStateChangedMessage : Message
    {
        public GameState gameState;
        
        public GameStateChangedMessage(GameState state)
            : base(MessageType.GameStateChanged)
        {
            this.gameState = state;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.GameStateChangedMessage(this);
        }
    }
    
    /**
     * See `Match` method for an explanation.
     */
    public class Matcher
    {
        public Action<ButtonMessage> ButtonMessage = _ => { };
        public Action<JoystickMessage> JoystickMessage = _ => { };
        public Action<PlayerColorMessage> PlayerColorMessage = _ => { };
        public Action<NameMessage> NameMessage = _ => {};
        public Action<NameTakenMessage> NameTakenMessage = _ => {};
        public Action<NameOkMessage> NameOkMessage = _ => {};
        public Action<MaxPlayersReachedMessage> MaxPlayersReachedMessage = _ => {};
        public Action<SetMenuActionMessage> SetMenuActionMessage = _ => {};
        public Action<MenuActionTriggeredMessage> MenuActionTriggeredMessage = _ => {};
        public Action<GameStateChangedMessage> GameStateChangedMessage = _ => {};
        public Action<SetGameActionMessage> SetGameActionMessage = _ => {};
    }

    /**
     * Using a switch-case on the message types is cumbersome since it requires
     * dynamic type checking, typecasts or a visitor pattern.
     *
     * Instead we simulate a "Match" functionality as it is used in functional
     * programming languages like Haskell or Scala for ADTs.
     *
     * See the `HandleMessage` method of `ControllerInput` for an usage example.
     *
     * @param matcher contains a callback action for each kind of message.
     */
    public virtual void Match(Matcher matcher)
    {
        throw new ApplicationException("You can not match on the message base class.");
    }
}

