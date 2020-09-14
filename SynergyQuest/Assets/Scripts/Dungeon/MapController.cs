using System;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{

    [SerializeField] private GameObject RoomViewPrefab;

    private Map map = new Map();
    
    public void Awake()
    {
        map.ParseDungeon();
        DrawMap();
        Debug.Log("Map Awake");
    }

    /**
     * TODO: Does not support slanted connections between rooms
     */
    private void DrawMap()
    {
        var grid = GetComponent<GridLayoutGroup>();
        Debug.Log(grid);
        grid.constraintCount = map.RoomMap.Count;
        foreach (var row in map.RoomMap)
        {
            foreach (var room in row)
            {
                var roomView = Instantiate(RoomViewPrefab, transform);
                
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


    private BoundsInt GetBounds(String sceneName)
    {
        return new BoundsInt(1,1,1,1,1,1);
        /*2
        var async = SceneManager.LoadSceneAsync(DungeonLayout.Instance.SceneNameFromRoomName(sceneName));
        async.allowSceneActivation = false;
        var scene = SceneManager.GetSceneByName(DungeonLayout.Instance.SceneNameFromRoomName(sceneName));
        GameObject[] gameObjects = scene.GetRootGameObjects();

        Tilemap tilemap = null;
        foreach (var gameObject in gameObjects)
        {
            tilemap = gameObject.GetComponentInChildren(typeof(Tilemap)) as Tilemap;
            if(tilemap != null) break;
        }

        SceneManager.UnloadSceneAsync(sceneName);
        return tilemap.cellBounds;
        */
    }
}