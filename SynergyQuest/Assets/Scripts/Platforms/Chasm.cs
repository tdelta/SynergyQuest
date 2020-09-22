// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

using System.Linq;
using ClipperLib;
using UnityEngine;

using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

/**
 * An object with a Chasm component marks an area where players fall and die.
 *
 * Hence, the object must also have either a BoxCollider2D or PolygonCollider2D component.
 */
public class Chasm : MonoBehaviour
{
    /**
     * This object is needed to perform set operations on shapes, see `OnTriggerStay2D` on why we need this.
     */
    private Clipper clipper = new Clipper();
    /**
     * The geometric shape of the chasm. It is computed in `Awake`
     */
    private Path _ownShape;
    /**
     * Keep a buffer for storing the geometric shape of a player, so that we do not have to allocate that memory every
     * physics frame.
     *
     * NOTE: Should the need for optimization arise, we can probably also keep buffers for the other shapes which are
     *       frequently allocated.
     */
    private Path _playerShapeBuffer = new Path( new IntPoint[4] );

    public void Awake()
    {
        // Compute the geometric shape of this chasm from its colliders
        _ownShape = Shapes.PathFromCollider(this.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // We now want to compute, whether the player is standing on the chasm, which means they should fall
        if (other.CompareTag("Player") || other.CompareTag("Item"))
        {
            // Compute some geometric properties of the player, e.g. area of its collider and the shape of the collider
            var playerBounds = other.GetComponent<BoxCollider2D>().bounds;
            var playerArea = playerBounds.size.x * playerBounds.size.y;
            Shapes.BoundsToPath(playerBounds, _playerShapeBuffer);

            // Get all platforms the player is currently standing on
            var platforms = other.GetComponent<PlatformTransportable>().PlatformsInContact;
            // Compute the union of the shapes of all platforms
            var platformsShape = Shapes.Union(
                platforms
                    .Select(platform => Shapes.PathFromCollider(platform.gameObject))
                    .ToList(),
                clipper
            );
            
            // Now compute the shape of the area, which the player collider and the chasm share,
            // i.e. intersect their shapes
            var playerChasmIntersection = Shapes.Intersection(
                _ownShape,
                _playerShapeBuffer,
                clipper
            );
            
            // Now subtract from that shape, the shape of the platforms the player is standing on.
            // Therefore, all that is then left is the shape of the area where nothing is between the player and the
            // chasm.
            playerChasmIntersection = Shapes.Difference(
                playerChasmIntersection,
                platformsShape
            );
            
            // Compute the area of this shape.
            var intersectionArea = Shapes.Area(playerChasmIntersection);

            // If the player is standing on a chasm with more than 50% of their own area, they fall
            if (intersectionArea / playerArea > 0.5)
            {
                // Get a handle on the player
                if (other.TryGetComponent<PlayerController>(out var playerController))
                    playerController.InitiateFall();
                else if (other.TryGetComponent<Item>(out var item))
                    item.gameObject.Shrink(new Vector3(0.01f, 0.01f, 0.01f));
            }
        }
    }

    public static bool IsOnChasm(Collider2D collider)
    {
        return collider.IsTouchingLayers(LayerMask.GetMask("Chasm"));
    }
}

