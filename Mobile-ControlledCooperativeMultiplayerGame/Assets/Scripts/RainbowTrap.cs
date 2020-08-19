using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

/**
 * A rainbow trap can be placed on a tilemap. When a player with a compatible
 * color steps on a rainbow trap, the trap changes colors.
 * When a player with an incompatible color steps on such a trip, it disappears
 * for a few seconds and the player falls into an abyss.
 */
public class RainbowTrap : MonoBehaviour
{
    [SerializeField] PlayerColor color = PlayerColor.Blue;

    Material _material;
    Path _ownPath;
    Collider2D _lastColliderOnTrap;
    bool hidden;
    int _tintSpeed = 2;
    int TrapColor = Shader.PropertyToID("_TrapColor");

    public Path Path => _ownPath;

    // Stuff needed to perform set operations on shapes
    Path _playerShapeBuffer = new Path(new IntPoint[4]);
    Clipper clipper = new Clipper();

    void Start()
    {
        _ownPath  = Shapes.PathFromCollider(gameObject);
        _material = GetComponent<Renderer>().material;
        _material.SetColor(TrapColor, PlayerColorMethods.ToRGB(color));
        
    }

    /**
     * if a player with a comptabile color steps on this trap significantly (share is > 0.5) remember it
     */
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() is PlayerController player && IsCompatibleWith(player.Color) &&
            ComputeOverlap(other.GetComponent<BoxCollider2D>().bounds) > 0.5)
        {
            _lastColliderOnTrap = other;
        }
    }

    /**
     * when the collider that leaves belongs to the remembered player, change to next color
     */
    void OnTriggerExit2D(Collider2D other)
    {
        if (!hidden && other.gameObject.CompareTag("Player") && _lastColliderOnTrap == other)
        {
            _lastColliderOnTrap = null;
            color = PlayerColorMethods.NextColor(color, PlayerDataKeeper.Instance.NumPlayers);
            _material.SetColor(TrapColor, PlayerColorMethods.ToRGB(color));
        }

    }

    /**
     * Compute overlap (between 0 & 1) of player and this trap
     */
    public double ComputeOverlap(Bounds playerBounds)
    {
        var playerArea = playerBounds.size.x * playerBounds.size.y;
        Shapes.BoundsToPath(playerBounds, _playerShapeBuffer);

        // compute intersection with bounds of player
        var playerTrapIntersection = Shapes.Intersection(
            _ownPath,
            _playerShapeBuffer,
            clipper);

        // return share of player on traps
        return Shapes.Area(playerTrapIntersection) / playerArea;
    }

    public bool IsCompatibleWith(PlayerColor color)
    {
        return !hidden && PlayerColorMethods.IsCompatibleWith(color, this.color);
    }

    public void Hide()
    {
        if (!hidden)
            StartCoroutine(HideAndShowAgain());
    }

    /**
     * hide trap with the alpha value of its color and slowly return to normal
     */
    IEnumerator HideAndShowAgain()
    {
      var hiddenColor = PlayerColorMethods.ToRGB(color);
      hiddenColor.a = 0;
      hidden = true;
      _material.SetColor(TrapColor, hiddenColor);
      yield return new WaitForSeconds(2);

      while (hiddenColor.a < 1)
      {
          hiddenColor.a += _tintSpeed * Time.fixedDeltaTime;
          _material.SetColor(TrapColor, hiddenColor);
          yield return new WaitForFixedUpdate();
      }
      hidden = false;
    }
}
