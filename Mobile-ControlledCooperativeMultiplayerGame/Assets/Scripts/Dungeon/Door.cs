using UnityEngine;

/**
 * This behavior should be placed on every door of a dungeon room.
 * 
 * It records the ID of the door, which is used by the `DungeonLayout` class to determine which room lies behind the
 * door.
 * Also it provides a method to load this room. This behavior may be used in conjunction with the `ContactSwitch`
 * behavior to trigger this method when a player touches the door.
 *
 * Furthermore, a transition type can be defined, which determines the transition animation to use when loading the next
 * room. See also the `TransitionController` and `SceneController` singletons.
 */
public class Door : MonoBehaviour
{
    /**
     * Identifier of this door. Should be the same as the one set in the layout file of the dungeon.
     */
    [SerializeField] private int doorId;
    /**
     * Which transition animation to play when using the door to switch scenes
     */
    [SerializeField] private TransitionType transitionType = TransitionType.None;

    public int DoorId => doorId;
    public TransitionType TransitionType => transitionType;

    /**
     * Use this door to load the next room.
     * A dungeon layout must be loaded for this to work, see also the `DungeonLayout` singleton.
     *
     * You can use an instance of the `DungeonLoader` behavior to ensure that the dungeon layout is loaded for a room.
     */
    public void UseDoor()
    {
        DungeonLayout.Instance.LoadRoomUsingDoor(this);
    }
}
