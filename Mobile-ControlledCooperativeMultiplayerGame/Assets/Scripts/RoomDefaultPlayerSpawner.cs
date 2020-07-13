/**
 * Spawns character objects for every connected controller at its current position.
 *
 * If debug mode is set to true, a character will also be spawned for every newly connected controller
 */
public class RoomDefaultPlayerSpawner : PlayerSpawner
{
    protected override bool IsSpawnerActive()
    {
        return !DungeonLayout.Instance.IsUsingDoorAsSpawner;
    }
}
