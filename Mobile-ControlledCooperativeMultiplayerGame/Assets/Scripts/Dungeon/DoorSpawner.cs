using UnityEngine;

/**
 * If players use a door to enter a room of a dungeon layout, they will be spawned by this spawner.
 * For this to work, an instance of this behavior should be associated with every door as a child object and placed
 * correctly, as players will be spawned at its location.
 */
public class DoorSpawner: PlayerSpawner
{
    /**
     * Set this field to the door this spawner belongs to
     */
    [SerializeField] private Door door;

    protected override bool IsSpawnerActive()
    {
        // The spawner should only be active, if it belongs to the door which has been used to enter the room
        return DungeonLayout.Instance.SpawnerDoorId == door.DoorId;
    }
}
