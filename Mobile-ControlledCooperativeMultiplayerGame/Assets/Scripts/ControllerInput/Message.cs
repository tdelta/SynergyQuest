using System;

public enum MessageType
{
    Button = 0,
    Joystick = 1
}

public enum Button
{
    Attack,
    Pull
}

[Serializable]
public class Message
{
    public MessageType type;
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