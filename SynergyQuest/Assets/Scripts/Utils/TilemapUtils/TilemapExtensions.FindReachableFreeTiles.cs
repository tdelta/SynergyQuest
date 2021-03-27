using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    public static partial class TilemapExtensions
    {
        /**
         * <summary>
         * Returns <see cref="numPositionsToFind"/> cell positions in the given tilemap which are reachable from
         * <see cref="worldPosition"/> when traversing the tilemap in a BFS while not traveling though tiles which
         * contain colliders.
         * </summary>
         */
        public static List<Vector2Int> FindReachableFreeTiles(
            this Tilemap tilemap,
            Vector3 worldPosition,
            int numPositionsToFind)
        {
            var cellPosition = tilemap.layoutGrid.WorldCellCenterToCell(worldPosition);
            return tilemap.FindReachableFreeTiles((Vector2Int) cellPosition, numPositionsToFind);
        }
        
        /**
         * <summary>
         * Returns <see cref="numPositionsToFind"/> cell positions in the given tilemap which are reachable from
         * <see cref="startCellPosition2d"/> when traversing the tilemap in a BFS while not traveling though tiles which
         * contain colliders.
         * </summary>
         */
        public static List<Vector2Int> FindReachableFreeTiles(this Tilemap tilemap, Vector2Int startCellPosition2d, int numPositionsToFind)
        {
            return tilemap
                .traverse(startCellPosition2d, new TraversalVisitor(numPositionsToFind))
                .ValueOr(new List<Vector2Int>());
        }
        

        /**
         * Same as <see cref="FindReachableFreeTiles(UnityEngine.Tilemaps.Tilemap,UnityEngine.Vector3,int)"/>, but
         * returns the world position of the center of the found tiles.
         */
        public static List<Vector3> FindReachableFreeTilesAsWorldPosition(this Tilemap tilemap, Vector3 startPosition, int numNeededPositions)
        {
            return tilemap.FindReachableFreeTiles(startPosition,
                    numNeededPositions
                )
                .Select(tilemapPosition =>
                {
                    return tilemap.layoutGrid.GetCellCenterWorld(tilemapPosition);
                })
                .ToList();
        }
        
        /**
         * Implements the BFS of <see cref="TilemapExtensions.FindReachableFreeTiles"/> using the generic BFS
         * algorithm of <see cref="TilemapBFS.traverse"/>.
         */
        private class TraversalVisitor : TilemapBFS.TraversalVisitor<List<Vector2Int>>
        {
            private readonly int numPositionsToFind;
            private readonly List<Vector2Int> foundPositions = new List<Vector2Int>();

            public TraversalVisitor(int numPositionsToFind)
            {
                this.numPositionsToFind = numPositionsToFind;
            }
            
            public TilemapBFS.TraversalDecision<List<Vector2Int>> decide(TilemapBFS.Node node)
            {
                // Is the tilemap filled here at all?
                if (node.tilemap.HasTilesAt2dPosition(node.cellPosition2D))
                {
                    // Is there space here for an object with a physics body?
                    if (node.tilemap.HasCollisionAt(node.cellPosition2D) == null)
                    {
                        // If not, we found a suitable tilemap position. Add it to the result
                        if (foundPositions.Count < numPositionsToFind)
                        {
                            foundPositions.Add(node.cellPosition2D);
                        }
                        
                        // Terminate, if we have found enough positions
                        if (foundPositions.Count >= numPositionsToFind)
                        {
                            return new TilemapBFS.TraversalDecision<List<Vector2Int>>.Terminate(foundPositions);
                        }

                        // Otherwise continue on neighbors
                        return new TilemapBFS.TraversalDecision<List<Vector2Int>>.Branch(
                            node.neighbors.Value
                        );
                    }
                }

                // else, ignore the cell
                return new TilemapBFS.TraversalDecision<List<Vector2Int>>.Prune();
            }

            public Optional<List<Vector2Int>> getPartialResult()
            {
                return Optional.Some(foundPositions);
            }
        }
    }
}