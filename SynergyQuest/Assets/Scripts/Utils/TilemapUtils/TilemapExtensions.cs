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

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    public static partial class TilemapExtensions
    {
        /**
     * <summary>
     * Tries to find a position in a grid cell close to the given position which is free of colliders.
     * If none can be found, returns the given position.
     * </summary>
     *
     * <param name="tilemap">Map which shall be searched for collider-free positions</param>
     * <param name="position">target position. We will try to find a free grid cell close to it</param>
     * <param name="maxChecks">how many cells we shall check before giving up</param>
     */
        public static Vector3 NearestFreeGridPosition(this Tilemap tilemap, Vector3 position, int maxChecks = 30)
        {
            var centerCell = tilemap.layoutGrid.WorldToCell(position);
            var toCheck = new Queue<Vector3Int>();
            toCheck.Enqueue(centerCell);
            var checkedPositions = new HashSet<Vector3Int>();

            var numCheckedCells = 0;
            var colliderBuffer = new Collider2D[1];
            while (toCheck.Any() && numCheckedCells < maxChecks)
            {
                var cell = toCheck.Dequeue();
                var worldPos = tilemap.GetCellCenterWorld(cell);
            
                if (
                    //Physics2D.OverlapCircleNonAlloc(worldPos, Mathf.Max(tilemap.cellSize.x, tilemap.cellSize.y), colliderBuffer) == 0
                    Physics2D.OverlapBoxNonAlloc(worldPos, tilemap.cellSize * 0.95f, 0, colliderBuffer) == 0
                )
                {
                    return new Vector3(worldPos.x, worldPos.y, position.z);
                }

                else
                {
                    checkedPositions.Add(cell);
                    var nextToCheck = new[]
                    {
                        cell + Vector3Int.up,
                        cell + Vector3Int.left,
                        cell + Vector3Int.right,
                        cell + Vector3Int.down,
                        cell + Vector3Int.up + Vector3Int.left,
                        cell + Vector3Int.left + Vector3Int.down,
                        cell + Vector3Int.right + Vector3Int.up,
                        cell + Vector3Int.down + Vector3Int.right,
                    };

                    foreach (var nextCell in nextToCheck.Where(nextCell => !checkedPositions.Contains(nextCell)))
                    {
                        toCheck.Enqueue(nextCell);
                    }
                }

                ++numCheckedCells;
            }

            return position;
        }

        /**
     * <summary>
     * Tries to find a tilemap object of name "Tilemap".
     * </summary>
     */
        [CanBeNull]
        public static Tilemap FindMainTilemap()
        {
            if (GameObject.Find("Tilemap") is GameObject tilemapObj &&
                tilemapObj.GetComponent<Tilemap>() is Tilemap tilemap)
            {
                return tilemap;
            }

            else
            {
                return null;
            }
        }

        /**
     * <summary>
     * Sometimes the collider of a <see cref="Tilemap"/> are not rebuilt when its contents are changed with
     * <see cref="Tilemap.SetTile"/> etc. This extension method forces the collider to fix itself.
     * </summary>
     */
        public static void RefreshColliders(this Tilemap tilemap)
        {
            if (tilemap.TryGetComponent(out TilemapCollider2D collider))
            {
                // Workaround to force regeneration of colliders is disabling and enabling the collider
                collider.enabled = false;
                // ReSharper disable once Unity.InefficientPropertyAccess
                collider.enabled = true;
            }

            else
            {
                Debug.LogError("Can not refresh collider of tilemap, since it does not have a collider.");
            }
        }

        /**
         * Get all valid 3d tile positions at the given 2d position.
         */
        public static IEnumerable<Vector3Int> ToValid3DPositions(this Tilemap tilemap, Vector2Int cell2dPosition)
        {
            var cellBounds = tilemap.cellBounds;
        
            return Enumerable
                .Range(cellBounds.zMin, cellBounds.size.z)
                .Select(zPosition => new Vector3Int(cell2dPosition.x, cell2dPosition.y, zPosition))
                .Where(tilemap.HasTile);
        }

        /**
         * Get all tiles in the tilemap at the given position
         */
        public static IEnumerable<TileT> AllTilesAt2dPosition<TileT>(this Tilemap tilemap, Vector2Int position2d)
            where TileT: TileBase
        {
            return tilemap
                .ToValid3DPositions(position2d)
                .Select(tilemap.GetTile<TileT>);
        }

        /**
         * Check if there are any tiles set at the given tile position.
         */
        public static bool HasTilesAt2dPosition(this Tilemap tilemap, Vector2Int position2d)
        {
            return tilemap.AllTilesAt2dPosition<TileBase>(position2d).Any();
        }

        /**
         * Check if there is any 2d object with a collider at the world position of the given tile position
         */
        public static Collider2D HasCollisionAt(this Tilemap tilemap, Vector2Int position2d)
        {
            var worldPos = tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(position2d.x, position2d.y, 0));

            return Physics2D.OverlapBox(worldPos, tilemap.cellSize * 0.95f, 0);
        }
    
        /**
         * Get cell position from world position
         */
        public static Vector2Int WorldCellCenterToCell(this Grid layoutGrid, Vector3 worldPosition)
        {
            return (Vector2Int) layoutGrid.WorldToCell(worldPosition);
        }

        /**
         * Get world position of the center of the given tile position
         */
        public static Vector3 GetCellCenterWorld(this Grid layoutGrid, Vector2Int cellPosition)
        {
            return layoutGrid.GetCellCenterWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0));
        }
    }
}