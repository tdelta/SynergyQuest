using UnityEngine;

public class DungeonLoader: MonoBehaviour
{
    [SerializeField] private string room;
    [SerializeField] private string filePath;
    [SerializeField] private int playerNum = -1;
        
    void Awake()
    {
        if (!DungeonLayout.Instance.IsLoaded)
        {
            var playerNum = this.playerNum == -1 ?
                  DebugSettings.Instance.DefaultDungeonPlayerNum
                : this.playerNum;
            
            DungeonLayout.Instance.LoadDungeon(filePath, playerNum, room, true);
        }
    }
}
