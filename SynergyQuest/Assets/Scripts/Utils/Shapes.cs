using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClipperLib;
using UnityEngine;
    
// A geometric shape is determined by one or multiple paths of points
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

/**
 * This collection of functions allows to perform set operations (Union, Intersection, Difference) on geomtric shapes.
 * It uses the "Clipper" library to perform the computations: http://angusj.com/delphi/clipper.php
 */
public static class Shapes
{
    /**
     * Determines how precise shape calculations shall be.
     * E.g. a value of 100 means, that the calculation will be precise to up to 2 decimal places 
     */
    private static long precision = 100;

    /**
     * Given a game object, compute its geometric shape from its colliders.
     * It must either have a `BoxCollider2D` or a `PolygonCollider2D`.
     */
    public static Path PathFromCollider(GameObject gameObject)
    {
        var boxCollider = gameObject.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            return BoundsToPath(boxCollider.bounds);
        }

        var polyCollider = gameObject.GetComponent<PolygonCollider2D>();
        if (polyCollider != null)
        {
            var position = (Vector2) gameObject.transform.position;
            return VectorsToPath(polyCollider.points.Select(vector => position + vector));
        }

        throw new NotSupportedException("Only BoxCollider2D and PolygonCollider2D are supported.");
    }

    /**
     * Computes the union of a list of shapes.
     *
     * @param paths   shapes to combine
     * @param clipper Clipper instance from the `Clipper` library. If none is provided, one is created.
     */
    public static Paths Union(List<Path> paths, Clipper clipper = null)
    {
        if (clipper == null)
        {
            clipper = new Clipper();
        }
        
        clipper.Clear();
        clipper.AddPaths(paths, PolyType.ptSubject, true);

        var solution = new Paths();
        clipper.Execute(ClipType.ctUnion, solution);

        return solution;
    }

    /**
     * Computes the difference of two shapes, i.e. how the first shape changes if all area shared with the second shape
     * is removed.
     *
     * @param clipper Clipper instance from the `Clipper` library. If none is provided, one is created.
     */
    public static Paths Difference(Paths subtrahend, Paths minuend, Clipper clipper = null)
    {
        if (clipper == null)
        {
            clipper = new Clipper();
        }
        
        clipper.Clear();
        clipper.AddPaths(subtrahend, PolyType.ptSubject, true);
        clipper.AddPaths(minuend, PolyType.ptClip, true);

        var solution = new Paths();
        clipper.Execute(ClipType.ctDifference, solution);

        return solution;
    }

    /**
     * Computes the intersection of two shapes.
     *
     * @param clipper Clipper instance from the `Clipper` library. If none is provided, one is created.
     */
    public static Paths Intersection(Path p1, Path p2, Clipper clipper = null)
    {
        if (clipper == null)
        {
            clipper = new Clipper();
        }
        
        clipper.Clear();
        clipper.AddPath(p1, PolyType.ptSubject, true);

        return intersect(p2, clipper);
    }

    public static Paths Intersection(Paths p1, Path p2, Clipper clipper = null)
    {
        if (clipper == null)
        {
            clipper = new Clipper();
        }
        
        clipper.Clear();
        clipper.AddPaths(p1, PolyType.ptSubject, true);

        return intersect(p2, clipper);
    }

    static Paths intersect(Path p , Clipper clipper)
    {
        clipper.AddPath(p, PolyType.ptClip, true);

        var solution = new Paths();
        clipper.Execute(ClipType.ctIntersection, solution);

        return solution;

    }
    
    /**
     * Computes the area of a shape
     */
    public static double Area(Paths paths)
    {
        // Sum up the area of all shapes.
        //
        // Note that we divide the area by the square of the precision. This is because when creating the shapes, all
        // floating point values are converted into integers by multiplying them with the precision.
        return paths.Sum(Clipper.Area) / (precision * precision);
    }

    /**
     * Converts a list of vectors into a path / shape.
     */
    public static Path VectorsToPath(IEnumerable<Vector2> points)
    {
        var path = new Path();
        path.AddRange(
            points.Select(VectorToIntPoint)
        );

        return path;
    }
    
    /**
     * Converts bounds into a rectangular path / shape.
     *
     * @param bounds  bounds to convert
     * @param outPath pre-allocated list to store the path in
     */
    public static void BoundsToPath(Bounds bounds, Path outPath)
    {
        outPath[0] = VectorToIntPoint(bounds.min);
        outPath[1] = VectorToIntPoint(bounds.min + Vector3.right * bounds.size.x);
        outPath[2] = VectorToIntPoint(bounds.max);
        outPath[3] = VectorToIntPoint(bounds.min + Vector3.up * bounds.size.y);
    }
    
    /**
     * Converts bounds into a rectangular path / shape.
     */
    private static Path BoundsToPath(Bounds bounds)
    {
        var path = new Path( new IntPoint[4] );

        BoundsToPath(bounds, path);

        return path;
    }

    private static IntPoint VectorToIntPoint(Vector3 vector)
    {
        return VectorToIntPoint((Vector2) vector);
    }

    private static IntPoint VectorToIntPoint(Vector2 vector)
    {
        var scaledVector = vector * precision;
        
        return new IntPoint(
            scaledVector.x,
            scaledVector.y
        );
    }
}

public static class ClipperLibExtensionMethods 
{
    public static string ToDebugString(this IntPoint point)
    {
        return $"({point.X}, {point.Y})";
    }
    
    public static string ToDebugString(this Path shape)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append('[');

        stringBuilder.Append(
            string.Join(", ", shape.Select(ToDebugString))
        );

        stringBuilder.Append(']');

        return stringBuilder.ToString();
    }
    
    public static string ToDebugString(this Paths shapes)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append('{');

        stringBuilder.Append(
            string.Join(", ", shapes.Select(ToDebugString))
        );

        stringBuilder.Append('}');

        return stringBuilder.ToString();
    }
}
