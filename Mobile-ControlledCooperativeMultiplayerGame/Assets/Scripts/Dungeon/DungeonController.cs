using System;


[Obsolete("Use `DungeonLayout` script instead")]
public class DungeonController : BehaviourSingleton<DungeonController>
{

    private int currentScene = -1;
    private string[] dungeon = {"TrapMaze", "Sokoban_4", "MonsterRoom", "Sokoban_2", "MonsterRoom", "Sokoban_3"};

    public void LoadNextRoom() {
        if (currentScene < dungeon.Length - 1) {
            SceneController.Instance.LoadSceneByName(dungeon[++currentScene]);
        } else {
            SceneController.Instance.LoadEndOfGame();
        }
    }
}
