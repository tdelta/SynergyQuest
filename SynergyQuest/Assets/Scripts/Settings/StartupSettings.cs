using UnityEngine;

[CreateAssetMenu(fileName = "StartupSettings", menuName = "ScriptableObjects/StartupSettings")]
public class StartupSettings: ScriptableObjectSingleton<StartupSettings>
{
    /**
     * <summary>
     * Name of the dungeon layout file which shall be loaded by the game when exiting the lobby and
     * starting the game.
     * </summary>
     */
    public string InitialDungeonLayoutFile => initialDungeonLayoutFile;
    [SerializeField] private string initialDungeonLayoutFile = "MainDungeon.json";
}