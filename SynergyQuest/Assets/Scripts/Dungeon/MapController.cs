using System;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{

    [SerializeField] private GameObject RoomViewPrefab;

    private Map map = new Map();
    
    public void Awake()
    {
        map.ParseDungeon();
    }

    private void Start()
    {
        DrawMap();
    }

    /**
     * TODO: Does not support slanted connections between rooms
     */
    private void DrawMap()
    {
        var grid = GetComponent<GridLayoutGroup>();
        // Force all layout groups to calculate their sizes so that we can retrieve the space we can draw in
        Canvas.ForceUpdateCanvases();
        
        var parentRect = ((RectTransform) grid.transform.parent).rect;
        
        var availableSpace = Mathf.Min(parentRect.width, parentRect.height);
        var cellSize = availableSpace / Mathf.Max(map.RowCount, map.ColumnCount);

        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = map.RowCount;
        
        grid.cellSize = new Vector2(cellSize, cellSize);
        
        foreach (var row in map.RoomMap)
        {
            foreach (var room in row)
            {
                var roomView = Instantiate(RoomViewPrefab, transform);
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
                        roomCenterView.GetComponent<Image>().color = Color.blue;
                    }
                    
                    if (r.HasLeftConnection()) roomLeftView.GetComponent<Image>().enabled = true;
                    if (r.HasRightConnection()) roomRightView.GetComponent<Image>().enabled = true;
                    if (r.HasTopConnection()) roomTopView.GetComponent<Image>().enabled = true;
                    if (r.HasBottomConnection()) roomBottomView.GetComponent<Image>().enabled = true;

                }, () => { });
            }
        }
    }
}