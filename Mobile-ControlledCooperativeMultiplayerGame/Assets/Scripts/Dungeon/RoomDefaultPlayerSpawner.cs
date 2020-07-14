/**
 * Spawns player prefab instances at its current position, if they have not entered the scene through a door.
 * 
 * This includes locally controlled debugging instances and remotely controlled instances.
 * It also respawns dead players and already existing players from a previous scene, if scenes have been switched.
 *
 * This behavior should be used for entry points to a dungeon, or to spawn players in intermediate dungeon rooms for
 * debugging purposes.
 * 
 * See the superclass for more information.
 */
public class RoomDefaultPlayerSpawner : PlayerSpawner
{
    protected override bool IsSpawnerActive()
    {
        // This spawner is only active if a players have not entered the current dungeon room through a door.
        return !DungeonLayout.Instance.IsUsingDoorAsSpawner;
    }
}
