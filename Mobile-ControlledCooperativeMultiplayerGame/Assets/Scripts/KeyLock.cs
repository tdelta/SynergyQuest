using UnityEngine;

/**
 * A switch which can be activated by using a key, see also the `Key` behaviour.
 * It is usually used in conjunction with doors.
 *
 * A game object implementing this behavior must also have a `Switch` component.
 * Moreover, you may want to use the `Interactive` component, to trigger the `Unlock` method of this behaviour.
 */
[RequireComponent(typeof(Switch))]
public class KeyLock : MonoBehaviour
{
    /**
     * Sprite of a key. It will be displayed during an animation while opening the lock.
     */
    [SerializeField] private Sprite keySprite = default;
    /**
     * Usually doors use a `ContactTrigger` to detect, whether they shall be opened. Since the player is already in
     * contact with a door while opening its lock, the trigger will not activate by itself, when opening the door.
     *
     * Hence, we use this reference to activate the trigger manually when opening this lock. Otherwise, the player would
     * have to step back from the door and forth again to open it, after unlocking.
     */
    [SerializeField] private ContactTrigger contactTriggerToReset = default;
    
    private Switch _switch;

    private void Awake()
    {
        _switch = GetComponent<Switch>();
    }

    /**
     * Unlock this lock, if it has not already been opened and if the players have enough keys.
     * The game will remember if a door has been unlocked across scenes, if a `DungeonLayout` has been loaded.
     *
     * @param unlocker the player unlocking this lock
     */
    public void Unlock(PlayerController unlocker)
    {
        // Only continue, if the lock has not been opened already and if the players have enough keys...
        if (!_switch.Value && PlayerDataKeeper.Instance.NumKeys > 0)
        {
            // Reduce the number of keys
            PlayerDataKeeper.Instance.NumKeys -= 1;
            
            // Open the lock, by activating its `Switch` component
            _switch.Value = true;
            
            // The player who unlocks this lock, shall present the key in an animation.
            unlocker.PresentItem(keySprite, () =>
            {
                // Let the `ContactTrigger` of the door refire contact events after the animation completed. See also
                // the description of the `contactTriggerToReset` field.
                if (contactTriggerToReset != null)
                {
                    contactTriggerToReset.RecheckContacts();
                }
            });
        }
    }
}
