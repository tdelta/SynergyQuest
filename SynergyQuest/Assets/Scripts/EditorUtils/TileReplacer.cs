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

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * <summary>
 * Modifies tiles around an object, when it is first placed in the editor.
 * </summary>
 * <remarks>
 * This is for example used with doors, which need to modify the wall tiles near to them.
 * </remarks>
 */
[ExecuteInEditMode]
public class TileReplacer : MonoBehaviour
{
    #if UNITY_EDITOR
        [Tooltip("Tilemap which is modified by this component. If not set, some tilemap object in the scene is selected automatically.")]
        [SerializeField] private Tilemap tilemap = default;
        [Tooltip("Which world position shall be used to determine the cell this object is placed in? If not set, the position of this game object is used instead.")]
        [SerializeField] private Transform tileCenterPosition = default;
        [Tooltip("Which tiles shall be (re-)placed where when this game object is added to the scene?")]
        [SerializeField] private TileReplacement[] replacements = new TileReplacement[0];
        /**
         * <summary>
         * Remembers, whether tiles have already been replaced. If so, no replacements will be performed anymore, when
         * this behaviour is loaded.
         * </summary>
         */
        [HideInInspector] [SerializeField] private bool replacementWasPerformed = false;
        public bool ReplacementWasPerformed => replacementWasPerformed;

        void Start()
        {
            if (!EditorApplication.isPlaying && !replacementWasPerformed && !this.IsPrefab())
            {
                PerformReplacement();
            }
        }

        public void PerformReplacement()
        {
            // Retrieve tilemap, if not set
            if (tilemap == null)
            {
                tilemap = FindObjectOfType<Tilemap>();
            }
            
            // Let editor know, we are about to perform changes to an object, so that an Undo action is available to the
            // user
            Undo.RecordObjects(new UnityEngine.Object[] {tilemap, this}, "Adapted Tilemap to door.");
            
            // In which cell of the tilemap are we located?
            var position = tilemap.WorldToCell(tileCenterPosition.position);
            
            // For every tile replacement we shall perform...
            foreach (var replacement in replacements)
            {
                // compute the target cell of the replacement relative to our current position
                var replacementPosition = new Vector3Int(
                    position.x + replacement.positionDelta.x,
                    position.y + replacement.positionDelta.y,
                    replacement.zPosition
                );

                // place the tile in the cell
                tilemap.SetTile(replacementPosition, replacement.replacementTile);
            }
            
            // If we do not do this, weird bugs can appear when using the Undo function in the editor:
            tilemap.RefreshAllTiles();
            // Without this, colliders might be wrong afterwards
            tilemap.RefreshColliders();

            replacementWasPerformed = true;
        }
    #endif
}

[Serializable]
public class TileReplacement
{
    [Tooltip("At what cell position shall the replacement occur relative to the placement of the object?")]
    public Vector2Int positionDelta = Vector2Int.zero;
    [Tooltip("At what Z position shall the placement occur?")]
    public int zPosition = 0;
    [Tooltip("Which tile shall be placed?")]
    public TileBase replacementTile = default;
}

#if UNITY_EDITOR
/**
 * <summary>
 * Custom inspector GUI for the <see cref="TileReplacer"/> behaviour.
 * It is the same as the default GUI, but a button is added, which allows to perform the replacement again.
 * </summary>
 */
[CustomEditor(typeof(TileReplacer))]
class TileReplacerEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var tileReplacer = (TileReplacer) target;
        GUILayout.Toggle(tileReplacer.ReplacementWasPerformed, "Replacement has been performed.");
            
        if (GUILayout.Button("Perform replacement (again)"))
        {
            tileReplacer.PerformReplacement();
        }
    }
}
#endif
