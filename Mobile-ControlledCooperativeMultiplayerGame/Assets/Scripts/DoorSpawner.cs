using UnityEngine;

public class DoorSpawner: PlayerSpawner
{
    [SerializeField] private Door door;

    protected override bool IsSpawnerActive()
    {
        return DungeonLayout.Instance.SpawnerDoorId == door.DoorId;
    }
}
