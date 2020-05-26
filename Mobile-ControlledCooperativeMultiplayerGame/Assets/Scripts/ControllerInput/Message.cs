using System;
using UnityEngine;

/**
 * Ids for the different kinds of messages which can be sent by the controller.
 * See the `Message` class hierarchy.
 */
public enum MessageType
{
    Button = 0,
    Joystick = 1
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
        }

        return null;
    }
}

[Serializable]
public class ButtonMessage : Message
{
    public Button button;
    public bool onOff;
}

[Serializable]
public class JoystickMessage : Message
{
    public float horizontal;
    public float vertical;
}