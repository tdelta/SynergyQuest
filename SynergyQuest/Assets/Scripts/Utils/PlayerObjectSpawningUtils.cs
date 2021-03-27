using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    /**
     * Functions for instantiating the player prefab, initializing and registering correctly.
     * The used prefab can be set in the <see cref="PrefabSettings"/> scriptable object singleton.
     * 
     * Also tries to cleverly place the instances in the tilemap close to the target position, while not triggering any
     * collisions and only considering reachable tiles.
     */
    public class PlayerObjectSpawningUtils
    {
        /**
         * Create <see cref="amount"/> player instances and try to place them near <see cref="targetPosition"/> as
         * explained in the description of <see cref="PlayerObjectSpawningUtils"/>.
         */
        public static List<PlayerController> InstantiatePlayerObjects(int amount, Vector3 targetPosition)
        {
            var spawnPositions = GetFreeSpawnPositions(targetPosition, amount);

            return spawnPositions
                .Select(spawnPosition => InstantiatePlayerObject(spawnPosition))
                .ToList();
        }
        
        /**
         * Instantiates a single player object at the given position.
         */
        private static PlayerController InstantiatePlayerObject(Vector3 spawnPosition)
        {
            var instance = Object.Instantiate(
                PrefabSettings.Instance.PlayerPrefab,
                // Ensure, z-coordinate is always 0 for players
                Vector3.Scale(spawnPosition, new Vector3(1, 1, 0)),
                Quaternion.identity
            );

            return instance;
        }
        
        /**
         * Try to determine <see cref="numNeededPositions"/> free positions in the tilemap without collisions while
         * only considering tiles which are reachable fromt <see cref="startPosition"/>.
         *
         * If not <see cref="numNeededPositions"/> free positions can be found, <see cref="startPosition"/> will be used
         * as fallback for the remaining positions.
         */
        private static List<Vector3> GetFreeSpawnPositions(Vector3 startPosition, int numNeededPositions)
        {
            var freePositions = new List<Vector3>();
            // Use BFS to find free positions
            if (TilemapExtensions.FindMainTilemap() is Tilemap tilemap)
            {
                freePositions.AddRange(
                    tilemap.FindReachableFreeTiles(
                            startPosition,
                            numNeededPositions
                        )
                        .Select(tilemapPosition =>
                        {
                            return tilemap.layoutGrid.GetCellCenterWorld(tilemapPosition);
                        })
                );
            }
            
            // Use startPosition as fallback, if not enough free positions could be found
            freePositions.AddRange(Enumerable.Repeat(startPosition, numNeededPositions - freePositions.Count));

            return freePositions;
        }
    }
}