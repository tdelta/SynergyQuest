#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace UnityEditor.Tilemaps
{
    
    /**
     * <summary>
     * Brush which allows to move tiles with different z-Values.
     * 
     * This class has been adapted from <c>GridBrush</c> which is part of a Unity Technologies project
     * (Tilemap Editor): https://docs.unity3d.com/Packages/com.unity.2d.tilemap@1.0/manual/index.html
     * Hence, this class is licensed under the Unity Companion License as a derivative work:
     * https://docs.unity3d.com/Packages/com.unity.2d.tilemap@1.0/license/LICENSE.html
     * </summary>
     */
    [CustomGridBrush(true, false, false, "Z Range Move Brush")]
    public class ZRangeMoveBrush : GridBrush
    {
        /**
         * Lowest z-value for which tiles are included in a move operation
         */
        [SerializeField] private int zStart = -5;
        /**
         * Highest z-value for which tiles are included in a move operation
         */
        [SerializeField] private int zEnd = 5;
        
        public override void MoveStart(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            Reset();
            UpdateSizeAndPivot(new Vector3Int(position.size.x, position.size.y, Math.Abs(zStart) + Math.Abs(zEnd) + 1), Vector3Int.zero);

            Tilemap tilemap = brushTarget.GetComponent<Tilemap>();
            if (tilemap == null)
                return;
            
            //Undo.RegisterCompleteObjectUndo(tilemap, "Cut tiles for move");

            foreach (Vector3Int pos in position.allPositionsWithin)
            {
                for (int z = zStart; z <= zEnd; ++z)
                {
                    Vector3Int brushPosition = new Vector3Int(pos.x - position.x, pos.y - position.y, z + (zStart < 0 ? Math.Abs(zStart) : 0));
                    var newPos = new Vector3Int(pos.x, pos.y, z);
                    PickCell(newPos, brushPosition, tilemap);
                    tilemap.SetTile(newPos, null);
                    tilemap.RefreshTile(newPos);
                }
            }
            
        }
        
        public override void MoveEnd(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            var newPos = position.min;
            newPos.z = zStart;
            
            Paint(gridLayout, brushTarget, newPos);
            
            if (brushTarget.TryGetComponent(out Tilemap tilemap))
            {
                tilemap.RefreshColliders();
            }
            
            Reset();
        }
        
        private void PickCell(Vector3Int position, Vector3Int brushPosition, Tilemap tilemap)
        {
            if (tilemap == null)
                return;

            SetTile(brushPosition, tilemap.GetTile(position));
            SetMatrix(brushPosition, tilemap.GetTransformMatrix(position));
            SetColor(brushPosition, tilemap.GetColor(position));
        }
    }
}
#endif