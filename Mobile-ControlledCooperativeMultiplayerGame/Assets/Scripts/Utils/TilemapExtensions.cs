using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapExtensions
{
    /**
     * Tries to find a position in a grid cell close to the given position which is free of colliders.
     * If none can be found, returns the given position.
     *
     * FIXME: Actually do an exhaustive search, currently, this just checks four neighboring cells
     */
    public static Vector3 NearestFreeGridPosition(this Tilemap tilemap, Vector3 position)
    {
        var centerCell = tilemap.layoutGrid.WorldToCell(position);
        var toCheck = new[]
        {
            centerCell,
            centerCell + Vector3Int.up,
            centerCell + Vector3Int.left,
            centerCell + Vector3Int.right,
            centerCell + Vector3Int.down,
            centerCell + Vector3Int.up + Vector3Int.left,
            centerCell + Vector3Int.left + Vector3Int.down,
            centerCell + Vector3Int.right + Vector3Int.up,
            centerCell + Vector3Int.down + Vector3Int.right,
        };

        var colliderBuffer = new Collider2D[1];
        foreach (var cell in toCheck)
        {
            var worldPos = tilemap.GetCellCenterWorld(cell);
            
            if (
                //Physics2D.OverlapCircleNonAlloc(worldPos, Mathf.Max(tilemap.cellSize.x, tilemap.cellSize.y), colliderBuffer) == 0
                Physics2D.OverlapBoxNonAlloc(worldPos, tilemap.cellSize * 0.95f, 0, colliderBuffer) == 0
            )
            {
                return new Vector3(worldPos.x, worldPos.y, position.z);
            }
        }

        return position;
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
}
