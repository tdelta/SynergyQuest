// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

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

    /**
     * The interactive is an optional component in this case, but if there is
     * one, make sure that it only creates speechbubbles and buttons when there
     * are enough keys.
     */
    private Interactive _interactive;

    private void Awake()
    {
        _switch = GetComponent<Switch>();
        _interactive = GetComponent<Interactive>();

        if (!ReferenceEquals(_interactive, null))
            _interactive.enabled = false;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (!ReferenceEquals(_interactive, null))
            _interactive.enabled = PlayerDataKeeper.Instance.NumKeys > 0;
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
