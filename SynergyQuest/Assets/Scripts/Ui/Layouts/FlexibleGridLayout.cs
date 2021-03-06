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

using UnityEngine;
using UnityEngine.UI;

/**
 * Grid layout which can dynamically adapt the size of cells.
 * It is based on the code from this video: https://www.youtube.com/watch?v=CGsEJToeXmA
 * with slight changes, bugfixes and more comments of whats going on.
 */
public class FlexibleGridLayout: LayoutGroup
{
    [SerializeField] private int rows = default;
    [SerializeField] private int columns = default;
    [SerializeField] private Vector2 cellSize = default;
    [SerializeField] private Vector2 spacing = default;
    [SerializeField] private FitType fitType = default;

    public int Rows
    {
        get => rows;
        set
        {
            rows = value;
        }
    }
    
    public int Columns
    {
        get => columns;
        set
        {
            columns = value;
        }
    }

    public enum FitType
    {
        // Try to achieve same number of columns and rows but this may result in empty space
        Uniform,
        // Fill all space, may result in more columns than rows
        MoreColumns,
        // Fill all space, may result in more rows than columns
        MoreRows,
        // Fill all space, use this fixed number of rows
        FixedRows,
        // Fill all space, use this fixed number of columns
        FixedColumns
    }
    
    /**
     * The minWidth, preferredWidth, and flexibleWidth values may be calculated in this callback.
     */
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        
        // If the number of rows or columns is not fixed, determine, what size the sides of a square need to have so
        // that it can contain all cells:
        // (So initially, we compute the number of rows and columns to be equal.)
        if (fitType != FitType.FixedColumns && fitType != FitType.FixedRows)
        {
            rows = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
            columns = rows;
        }

        switch (fitType)
        {
            case FitType.MoreColumns:
            case FitType.FixedColumns:
                rows = Mathf.CeilToInt(transform.childCount / (float) columns);
                break;
            case FitType.MoreRows:
            case FitType.FixedRows:
                columns = Mathf.CeilToInt(transform.childCount / (float) rows);
                break;
        }
        
        // Next, we compute the size of each individual cell by dividing the size of this layout element, given by the
        // parent by the number of columns / rows respectively
        var parentSize = rectTransform.rect;
        var padding = this.padding;
        cellSize.x =
            -spacing.x + // in every column, we need to subtract spacing from the cell width
            (parentSize.width
             + spacing.x // there is actually one less spacing than columns, so we restore that fraction of spacing
             - (padding.left + padding.right) // we need to subtract some width from every cell to get space for the padding
            ) / columns;
        cellSize.y =
            -spacing.y + // for every row, we need to subtract spacing from the cell height
            (parentSize.height
             + spacing.y // there is actually one less spacing than rows, so we restore that fraction of spacing
             - (padding.top + padding.bottom) // we need to subtract some height from every cell to get space for the padding
            ) / rows;
        
        // Now we can position every element according to its cell.
        // Elements are assigned cells based on their position in the object hierarchy
        int cell = 0;
        foreach (var child in rectChildren)
        {
            // Compute the row and column from the current cell index
            var currentColumn = cell % columns;
            var currentRow = cell / columns;
            
            // From the size of the cells and the above column / row number, we can compute the supposed position of the
            // child
            var positionX =
                  currentColumn * cellSize.x // Add up cell widths of columns
                + currentColumn * spacing.x // Add up spacing between columns
                + padding.left; // Add padding
            var positionY =
                  currentRow * cellSize.y // Add up cell heights of rows
                + currentRow * spacing.y // Add up spacing between rows
                + padding.top; // Add padding
            
            // Set x position
            SetChildAlongAxis(
                child,
                0, // x axis
                positionX,
                cellSize.x // dictate width of the child
            );
            
            // Set y position
            SetChildAlongAxis(
                child,
                1, // y axis
                positionY,
                cellSize.y // dictate height of the child
            );

            ++cell;
        }
    }

    /**
     * The minHeight, preferredHeight, and flexibleHeight values may be calculated in this callback.
     */
    public override void CalculateLayoutInputVertical()
    { }

    /**
     * Callback invoked by the auto layout system which handles horizontal aspects of the layout.
     */
    public override void SetLayoutHorizontal()
    { }

    /**
     * Callback invoked by the auto layout system which handles vertical aspects of the layout.
     */
    public override void SetLayoutVertical()
    { }
}
