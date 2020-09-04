using UnityEngine;

/**
 * Behavior which tries to load a dungon layout on Awake, if none is loaded already.
 * It does not reload the current scene though, as it assumes the current scene is already a dungeon room.
 *
 * The main purpose of this behavior is to load a dungeon layout while debugging/developing a room scene in the Unity
 * editor.
 */
public class DungeonLoader: MonoBehaviour
{
    /**
     * Name of the dungeon room the current scene is representing
     */
    [SerializeField] private string room = default;
    /**
     * Path to the JSON layout file describing the dungeon
     */
    [SerializeField] private string filePath = default;
    /**
     * Number of players for which the dungeon will be loaded.
     * If set to -1, `DebugSettings.Instance.DefaultDungeonPlayerNum` will be used instead.
     */
    [SerializeField] private int playerNum = -1;
        
    void Awake()
    {
        // If no dungeon layout has been loaded yet...
        if (!DungeonLayout.Instance.IsLoaded)
        {
            // Determine the number of players for which the dungeon shall be loaded
            // (see field description of `playerNum`)
            var playerNum = this.playerNum == -1 ?
                  DebugSettings.Instance.DefaultDungeonPlayerNum
                : this.playerNum;
            
            // Load the layout file of the dungeon
            DungeonLayout.Instance.LoadDungeon(
                filePath,
                playerNum,
                room,
                true // as we assume the current scene is already representing the given room, we do not
                                   // want to reload it.
            );
        }
    }
}
