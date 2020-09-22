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
using UnityEngine.Serialization;
using UnityEngine.UI;

/**
 * <summary>
 * Controls the UI of the map screen.
 * It gives an overview over the dungeon layout.
 * </summary>
 * <remarks>
 * To actually launch the map screen, see the <see cref="MapScreenLauncher"/> singleton.
 * </remarks>
 * <seealso cref="Map"/>
 * <seealso cref="DungeonLayout"/>
 */
public class MapScreenUi : MonoBehaviour, MenuUi
{
    [FormerlySerializedAs("RoomViewPrefab")]
    [SerializeField] private GameObject roomViewPrefab = default;
    [SerializeField] private GridLayoutGroup grid = default;

    private Map map = new Map();

    /**
     * TODO: Does not support slanted connections between rooms
     */
    private void DrawMap()
    {
        // Force all layout groups to calculate their sizes so that we can retrieve the space we can draw in
        Canvas.ForceUpdateCanvases();
        var parent = grid.transform.parent.GetComponent<LayoutGroup>();
        parent.CalculateLayoutInputHorizontal();
        parent.CalculateLayoutInputVertical();

        var parentTransform = parent.GetComponent<RectTransform>();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentTransform);
        
        var parentRect = ((RectTransform) parent.transform).rect;
        
        var availableSpace = Mathf.Min(parentRect.width, parentRect.height) - 100;
        var cellSize = availableSpace / Mathf.Max(map.RowCount, map.ColumnCount);

        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = map.RowCount;
        
        grid.cellSize = new Vector2(cellSize, cellSize);
        
        foreach (var row in map.RoomMap)
        {
            foreach (var room in row)
            {
                var roomView = Instantiate(roomViewPrefab, grid.transform);
                roomView.transform.localScale = new Vector3(
                    cellSize / ((RectTransform) roomView.transform).rect.width,
                    cellSize / ((RectTransform) roomView.transform).rect.height,
                    1
                );
                
                var roomLeftView = roomView.transform.GetChild(0);
                var roomRightView = roomView.transform.GetChild(1);
                var roomTopView = roomView.transform.GetChild(2);
                var roomBottomView = roomView.transform.GetChild(3);
                var roomCenterView = roomView.transform.GetChild(4);

                room.Match(r =>
                {
                    if (r.hasRoom) roomCenterView.GetComponent<Image>().enabled = true;

                    if (r.name == DungeonLayout.Instance.CurrentRoom)
                    {
                        // Room is current room => Indicate through special color
                        roomCenterView.GetComponent<Image>().color = new Color(0.827f, 0.239f, 0.125f);
                    }
                    
                    if (r.HasLeftConnection()) roomLeftView.GetComponent<Image>().enabled = true;
                    if (r.HasRightConnection()) roomRightView.GetComponent<Image>().enabled = true;
                    if (r.HasTopConnection()) roomTopView.GetComponent<Image>().enabled = true;
                    if (r.HasBottomConnection()) roomBottomView.GetComponent<Image>().enabled = true;

                }, () => { });
            }
        }
    }

    public void OnResumePressed()
    {
        MapScreenLauncher.Instance.Close();
    }

    public void OnLaunch()
    {
        map.ParseDungeon();
        DrawMap();
        
        // When the pause screen UI is opened, give remote controllers the capability to close the pause screen and resume the game
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, true),
            (MenuAction.ShowMap, false)
        );
    }

    public void OnClose()
    {
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, false),
            (MenuAction.ShowMap, true)
        );
    }

    public void OnMenuActionTriggered(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.ResumeGame:
                OnResumePressed();
                break;
        }
    }
}