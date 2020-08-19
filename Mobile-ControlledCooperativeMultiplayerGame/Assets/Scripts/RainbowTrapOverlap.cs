using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

/**
 * This component keeps track of all traps the object currently touches so that this information can be used by the
 * "RainbowTrap" class to determine whether a player should or shouldn't fall down a trap.
 */
public class RainbowTrapOverlap : MonoBehaviour
{
    // RainbowTraps this game object is currently in contact with
    HashSet<RainbowTrap> _traps = new HashSet<RainbowTrap>();
    PlayerController _playerController;

    // Stuff needed to perform set operations on shapes
    Path _playerShapeBuffer = new Path( new IntPoint[4] );
    Clipper clipper = new Clipper();

    void Start()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If the object we touched is a RainbowTrap, register it in the list of traps we are currently in
        // contact with.
        if (other.GetComponent<RainbowTrap>() is RainbowTrap trap)
            _traps.Add(trap);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // If the object we stopped touching is a RainbowTrap, unregister it from the list of traps we are
        // currently in contact with.
        if (other.GetComponent<RainbowTrap>() is RainbowTrap trap)
            _traps.Remove(trap);
    }

    /**
     * Compute overlap (between 0 & 1) of player and all incompatible traps
     * the player stands on
     */
    public double ComputeOverlap()
    {
        var playerBounds = GetComponent<BoxCollider2D>().bounds;
        var playerArea = playerBounds.size.x * playerBounds.size.y;
        Shapes.BoundsToPath(playerBounds, _playerShapeBuffer);

        // Compute the union of the shapes of all traps the player is standing on
        var trapsShape = Shapes.Union(
            _traps
                .Where(trap => !trap.IsCompatibleWith(_playerController.Color))
                .Select(trap => trap.Path)
                .ToList(),
            clipper
        );

        // compute intersection with bounds of players
        var playerTrapIntersection = Shapes.Intersection(
            trapsShape,
            _playerShapeBuffer,
            clipper
        );

        // return share of player on traps
        return Shapes.Area(playerTrapIntersection) / playerArea;
    }

    /**
     * If more than 50% of the player collider are on incompatible traps, let
     * him fall and hide the trap with the biggest share
     */
    void OnTriggerStay2D(Collider2D other)
    {
        if (_traps.Count > 0 && ComputeOverlap() > 0.5)
        {
            _playerController.InitiateFall();
            _traps
                .Where(trap => !trap.IsCompatibleWith(_playerController.Color))
                .OrderBy(trap => trap.ComputeOverlap(GetComponent<BoxCollider2D>().bounds))
                .Last()
                .Hide();
        }
    }
}
