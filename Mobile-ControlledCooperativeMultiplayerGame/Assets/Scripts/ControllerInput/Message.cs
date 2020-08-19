using System;
using System.Collections.Generic;
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
    SetMenuActions = 8,      // Set the set of enabled menu actions, sent by game
    MenuActionTriggered = 9, // A menu action has been selected on the controller
    GameStateChanged = 10,   // The state of the game changed, e.g. Lobby -> Game started. Sent by the game 
    VibrationSequence = 11,  // The game wants the controller to vibrate. Sent by the game
    SetEnabledButtons = 12,  // Enable / disable buttons in the controller UI, sent by game
    SetCooldownButtons = 13, // Mark buttons as "cooling down". The buttons are still enabled, but can currently not be used, since the action has a cooldown, sent by game
    IMUOrientation = 14,     // Orientation of the controller in 3D space (roll and pitch) interpreted as horizontal and vertical movement in 2D space. Sent by controller
    InputModeChanged = 15,   // Hint, which inputs the game currently expects. Sent by game. See also `InputMode`.
    PlayerInfo = 16          // Information about the player (health)
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
            
            case MessageType.IMUOrientation:
                return JsonUtility.FromJson<IMUOrientationMessage>(str);
            
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
            
            case MessageType.SetMenuActions:
                return JsonUtility.FromJson<SetMenuActionsMessage>(str);
            
            case MessageType.MenuActionTriggered:
                return JsonUtility.FromJson<MenuActionTriggeredMessage>(str);
            
            case MessageType.GameStateChanged:
                return JsonUtility.FromJson<GameStateChangedMessage>(str);
            
            case MessageType.InputModeChanged:
                return JsonUtility.FromJson<InputModeChangedMessage>(str);

            case MessageType.SetEnabledButtons:
                return JsonUtility.FromJson<SetEnabledButtonsMessage>(str);
                
            case MessageType.SetCooldownButtons:
                return JsonUtility.FromJson<SetCooldownButtonsMessage>(str);
            
            case MessageType.VibrationSequence:
                return JsonUtility.FromJson<VibrationSequenceMessage>(str);

            case MessageType.PlayerInfo:
                return JsonUtility.FromJson<PlayerInfoMessage>(str);
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
    public sealed class SetEnabledButtonsMessage : Message
    {
        public List<Button> enabledButtons;

        public SetEnabledButtonsMessage(List<Button> enabledButtons)
            : base(MessageType.SetEnabledButtons)
        {
            this.enabledButtons = enabledButtons;
        }
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.SetEnabledButtonsMessage(this);
        }
    }
    
    [Serializable]
    public sealed class SetCooldownButtonsMessage : Message
    {
        public List<Button> cooldownButtons;

        public SetCooldownButtonsMessage(List<Button> cooldownButtons)
            : base(MessageType.SetCooldownButtons)
        {
            this.cooldownButtons = cooldownButtons;
        }
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.SetCooldownButtonsMessage(this);
        }
    }

    [Serializable]
    public sealed class SetMenuActionsMessage : Message
    {
        public List<MenuAction> menuActions;
        
        public SetMenuActionsMessage(List<MenuAction> actions)
            : base(MessageType.SetMenuActions)
        {
            this.menuActions = actions;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.SetMenuActionsMessage(this);
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
    
    [Serializable]
    public sealed class InputModeChangedMessage : Message
    {
        public InputMode inputMode;
        
        public InputModeChangedMessage(InputMode inputMode)
            : base(MessageType.InputModeChanged)
        {
            this.inputMode = inputMode;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.InputModeChangedMessage(this);
        }
    }
    
    [Serializable]
    public sealed class VibrationSequenceMessage : Message
    {
        /**
         * Indicates how the controller shall vibrate.
         * The first number is the number of milliseconds to vibrate,
         * the next is the number to milliseconds to pause,
         * the number after that is again a number of milliseconds to vibrate and so on.
         *
         * Hence these are numbers of milliseconds to vibrate and pause in
         * alteration.
         */
        public List<float> vibrationPattern;
        
        public VibrationSequenceMessage(List<float> vibrationPattern)
            : base(MessageType.VibrationSequence)
        {
            this.vibrationPattern = vibrationPattern;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.VibrationSequenceMessage(this);
        }
    }
    
    [Serializable]
    public sealed class IMUOrientationMessage : Message
    {
        public float vertical;
        public float horizontal;
        
        public IMUOrientationMessage(float vertical, float horizontal)
            : base(MessageType.IMUOrientation)
        {
            this.vertical = vertical;
            this.horizontal = horizontal;
        }
        
        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.IMUOrientationMessage(this);
        }
    }
        
    [Serializable]
    public sealed class PlayerInfoMessage : Message
    {
        /**
         * Used to send data about the player (health, gold) to the controller
         */
        public PlayerInfo playerInfo;
        
        public PlayerInfoMessage(PlayerInfo info)
            : base(MessageType.PlayerInfo)
        {
            this.playerInfo = info;
        }

        /**
         * See base class method for an explanation.
         */
        public override void Match( Matcher matcher )
        {
            matcher.PlayerInfoMessage(this);
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
        public Action<SetMenuActionsMessage> SetMenuActionsMessage = _ => {};
        public Action<MenuActionTriggeredMessage> MenuActionTriggeredMessage = _ => {};
        public Action<GameStateChangedMessage> GameStateChangedMessage = _ => {};
        public Action<InputModeChangedMessage> InputModeChangedMessage = _ => {};
        public Action<SetEnabledButtonsMessage> SetEnabledButtonsMessage = _ => {};
        public Action<SetCooldownButtonsMessage> SetCooldownButtonsMessage = _ => {};
        public Action<VibrationSequenceMessage> VibrationSequenceMessage = _ => {};
        public Action<IMUOrientationMessage> IMUOrientationMessage = _ => {};
        public Action<PlayerInfoMessage> PlayerInfoMessage = _ => {};
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

