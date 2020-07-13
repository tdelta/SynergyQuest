using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private int doorId;
    [SerializeField] private TransitionType transitionType = TransitionType.None;
    

    public int DoorId => doorId;
    public TransitionType TransitionType => transitionType;

    public void UseDoor()
    {
        DungeonLayout.Instance.LoadRoomUsingDoor(this);
    }

    public DoorSpawner GetSpawner()
    {
        return GetComponentInChildren<DoorSpawner>();
    }
}
