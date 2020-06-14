using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerColor: int
{
    // Do not change the numeric values. The controller clients use the same values.
    Blue = 0,
    Yellow = 1,
    Red = 2,
    Green = 3,
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
     * Blue -> Yellow -> Red -> Green -> Blue
     * Any -> Any
     */
    public static PlayerColor NextColor(this PlayerColor color)
    {
        switch (color)
        {
            case PlayerColor.Any:
                return PlayerColor.Any;
            case PlayerColor.Blue:
                return PlayerColor.Yellow;
            case PlayerColor.Yellow:
                return PlayerColor.Red;
            case PlayerColor.Red:
                return PlayerColor.Green;
            case PlayerColor.Green:
                return PlayerColor.Blue;
        }

        return PlayerColor.Any;
    }
}