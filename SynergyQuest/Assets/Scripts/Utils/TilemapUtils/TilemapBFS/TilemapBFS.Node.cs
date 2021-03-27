using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    public static partial class TilemapBFS
    {
        /**
         * <summary>
         * Represents one visited tile in the tilemap.
         * Only provides tile position and links to neighbors.
         *
         * To get more detailed information about the tile, query the tilemap using the position.
         * </summary>
         */
        public class Node
        {
            public readonly Tilemap tilemap;
            
            public readonly Vector2Int cellPosition2D;
            
            // Only use this neighbor list to evaluate which neighbor nodes the algorithm should branch on
            // (see <see cref="TraversalDecision.Branch">)
            public readonly Lazy<List<Node>> neighbors;

            // Set of already visited nodes. Those will not be considered when computing the list of neighbors
            private readonly HashSet<Vector2Int> seen;
            
            public Node(Tilemap tilemap, Vector2Int cellPosition, HashSet<Vector2Int> seen)
            {
                this.tilemap = tilemap;
                this.cellPosition2D = cellPosition;
                this.neighbors = new Lazy<List<Node>>(() =>
                    new[]
                        {
                            cellPosition + Vector2Int.up,
                            cellPosition + Vector2Int.left,
                            cellPosition + Vector2Int.right,
                            cellPosition + Vector2Int.down,
                            cellPosition + Vector2Int.up + Vector2Int.left,
                            cellPosition + Vector2Int.left + Vector2Int.down,
                            cellPosition + Vector2Int.right + Vector2Int.up,
                            cellPosition + Vector2Int.down + Vector2Int.right,
                        }
                        .Where(position => !seen.Contains(position))
                        .Select(position => new Node(tilemap, position, seen))
                        .ToList()
                );
            }
        }
    }
}