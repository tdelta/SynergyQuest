using System.Collections.Generic;
using UnityEngine;

public class DungeonController : BehaviourSingleton<DungeonController>
{

    private int currentScene = -1;
    private string[] dungeon = {"Sokoban_1", "MonsterRoom", "Sokoban_2", "MonsterRoom", "Sokoban_3"};

    public void LoadNextRoom() {
        if (currentScene < dungeon.Length - 1) {
            SceneController.Instance.LoadSceneByName(dungeon[++currentScene]);
        } else {
            SceneController.Instance.LoadEndOfGame();
        }
    }
}
