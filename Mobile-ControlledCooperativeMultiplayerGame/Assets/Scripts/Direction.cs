    
using System;
using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

/**
 * Extension methods related to the Direction Enum
 */
public static class DirectionMethods
{
    /**
     * Converts a direction into a 2d base vector.
     */
    public static Vector2 ToVector(this Direction direction) {
        switch (direction)
        {
            case Direction.Left:
                return Vector2.left;
            case Direction.Right:
                return Vector2.right;
            case Direction.Up:
                return Vector2.up;
            case Direction.Down:
                return Vector2.down;
        }
        
        throw new NotImplementedException("This direction can not be converted into a vector.");
    }
    
    /**
     * Reverses a direction.
     * Up <-> Down
     * Left <-> Right
     */
    public static Direction Inverse(this Direction direction)
    {
        switch (direction) {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
        }
        
        throw new NotImplementedException("Cant inverse this direction.");
    }
    
    /**
     * Converts unit coordinate axis vectors into directions.
     *
     * @param this/vector vector to be converted. Must be one of Vector2.up, Vector2.down, ...
     * @param direction   used to store the direction the vector has been converted into, if possible.
     *                    Otherwise null.
     * @returns           Whether the conversion was successful.
     */
    public static bool ToDirection(this Vector2 vector, out Direction direction) {
        if (vector == Vector2.down)
        {
            direction = Direction.Down;
        }
        
        else if (vector == Vector2.up)
        {
            direction = Direction.Up;
        }
        
        else if (vector == Vector2.left)
        {
            direction = Direction.Left;
        }
        
        else if (vector == Vector2.right)
        {
            direction = Direction.Right;
        }

        else
        {
            direction = Direction.Left;
            return false;
        }

        return true;
    }
}
