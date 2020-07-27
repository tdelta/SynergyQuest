    
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

    /**
     * Given a bounded object and a point, this method returns the direction where the point lies relative to the bounds.
     *
     * @returns false if `other` lies in the center of the bounds, true otherwise
     */
    public static bool DirectionTo(this Bounds bounds, Vector3 other, out Direction direction)
    {
        if (bounds.center == other)
        {
            direction = Direction.Down;
            return false;
        }
        
        if (other.y > bounds.min.y && other.y < bounds.max.y)
        {
            direction = other.x < bounds.center.x ? Direction.Left : Direction.Right;
        }

        else
        {
            direction = other.y < bounds.center.y ? Direction.Down : Direction.Up;
        }

        return true;
    }
}
