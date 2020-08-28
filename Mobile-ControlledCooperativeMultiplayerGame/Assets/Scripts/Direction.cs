    
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
     * Given a vector, search for the closest direction it points to.
     * This is different from the `ToDirection` method, which only works for base vectors (Vector2.up, Vector2.left, ...) 
     */
    public static Direction ApproximateDirection(this Vector2 vector)
    {
        var directions = (Direction[]) Enum.GetValues(typeof(Direction));

        // Select direction with highest cosine similarity of base vector
        return directions.MaxBy(direction => Vector2.Dot(
            direction.ToVector(),
            vector)
        );
    }

    /**
     * Given a point, this method returns the direction where it lies relative to this bounds.
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

        // We set the center of bounds as the origin of a coordinate system in order to calculate angles
        var center = new Vector3(bounds.center.x, bounds.center.y, 0);
        var relativePoint = other - center;
        relativePoint.z = 0;
        
        // Edges of bounds
        var upperLeft = new Vector3(bounds.min.x, bounds.min.y,0) - center;
        var upperRight = new Vector3(bounds.max.x, bounds.min.y,0) - center;

        // Angles between edges of bounds and other
        var upperLeftAngle = Vector3.Angle(upperLeft, relativePoint);
        var upperRightAngle = Vector3.Angle(upperRight, relativePoint);


        if (upperLeftAngle < 90 && upperRightAngle < 90) direction = Direction.Down;
        else if (upperLeftAngle < 90 && upperRightAngle > 90) direction = Direction.Left;
        else if (upperLeftAngle > 90 && upperRightAngle < 90) direction = Direction.Right;
        else direction = Direction.Up;

        return true;
    }
}
