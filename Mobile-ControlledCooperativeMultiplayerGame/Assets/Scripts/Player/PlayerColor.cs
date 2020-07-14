using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerColor: int
{
    // Do not change the numeric values. The controller clients use the same values.
    Red = 0,
    Blue = 1,
    Green = 2,
    Yellow = 3,
    Any = 4 // May interact with any object.
}

public static class PlayerColorMethods
{
    /**
     * Determines whether game objects of different color may interact.
     * For example, a red box may only be pushed by the red player.
     *
     * The color `Any` may interact with any object.
     */
    public static bool IsCompatibleWith(this PlayerColor thisColor, PlayerColor otherColor)
    {
        return thisColor == PlayerColor.Any || otherColor == PlayerColor.Any || thisColor == otherColor;
    }

    /**
     * Allows to cycle through color values. This method is used whenever a new color must be assigned repeatedly, for
     * example when the game lobby assigns every player a different color.
     *
     * Order in which the values are cycled through:
     * 
     * Red -> Blue -> Green -> Yellow -> Red
     * Any -> Any
     */
    public static PlayerColor NextColor(this PlayerColor color)
    {
        switch (color)
        {
            case PlayerColor.Any:
                return PlayerColor.Any;
            case PlayerColor.Red:
                return PlayerColor.Blue;
            case PlayerColor.Blue:
                return PlayerColor.Green;
            case PlayerColor.Green:
                return PlayerColor.Yellow;
            case PlayerColor.Yellow:
                return PlayerColor.Red;
        }

        return PlayerColor.Any;
    }

    /*
     * Get RGB values for the player colors. Can be displayed in a material
     */
    public static Color ColorToRGB(PlayerColor color)
    {
        switch (color)
        {
            case PlayerColor.Red:
                return new Color(1.0f,0.52549f,0.48627f);
            case PlayerColor.Blue:
                return new Color(0.41569f,0.71765f,1.0f);
            case PlayerColor.Green:
                return new Color(0.46275f,0.82353f,0.45882f);
            case PlayerColor.Yellow:
                return new Color(1.0f,1.0f,0.41961f);
        }

        return Color.gray;
    }
}
