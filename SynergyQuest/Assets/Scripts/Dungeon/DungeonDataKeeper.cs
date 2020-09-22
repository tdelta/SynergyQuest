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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

/**
 * Keeps track of dungeon related runtime data which is persistent across scenes.
 */
public class DungeonDataKeeper: Singleton<DungeonDataKeeper>
{
    /**
     * Stores activation values of switches
     */
    private Dictionary<string, bool> _persistentSwitchValues = new Dictionary<string, bool>();
    
    /**
     * Stores whether chests have been opened or not
     */
    private Dictionary<string, bool> _persistentChestValues = new Dictionary<string, bool>();

    /**
     * Stores positions of objects
     */
    private Dictionary<string, Vector3> _objectPositions  = new Dictionary<string, Vector3>();
    
    /**
     * This set remembers objects which have been destroyed and shall stay destroyed when their scene is loaded again.
     * See also the `PersistentlyDestructible` behavior.
     */
    private HashSet<string> _destroyedObjects = new HashSet<string>();
    
    /**
     * This set remembers, which keys have been collected
     */
    private HashSet<string> _collectedKeys = new HashSet<string>(); // Key IDs
    
    /**
     * Returns the saved value of the given switch
     *
     * @param switcher     switch for which the activation shall be looked up
     * @param defaultValue if no data has been saved for the given switch, this value is returned.
     *                     (optional)
     */
    [Pure]
    public bool HasSwitchBeenActivated(Switch switcher, bool defaultValue = false)
    {
        if (_persistentSwitchValues.TryGetValue(switcher.Guid.guid, out var value))
        {
            return value;
        }

        else
        {
            return defaultValue;
        }
    }
    
    /**
     * Save the activation value of a switch
     */
    public void SaveSwitchActivation(Switch switcher)
    {
        _persistentSwitchValues[switcher.Guid.guid] = switcher.Value;
    }
    
    /**
     * Returns whether a chest has been opened or not, if the value has been saved with
     * SaveChestActivation.
     *
     * @param chest        chest for which the state shall be looked up
     * @param defaultValue if no data has been saved for the given chest, this value is returned.
     *                     (optional, default false)
     */
    [Pure]
    public bool HasChestBeenOpened(Chest chest, bool defaultValue = false)
    {
        if (_persistentChestValues.TryGetValue(chest.Guid.guid, out var value))
        {
            return value;
        }

        else
        {
            return defaultValue;
        }
    }
    
    /**
     * Save whether a chest has been opened or not
     */
    public void SaveChestActivation(Chest chest, bool value)
    {
        _persistentChestValues[chest.Guid.guid] = value;
    }
    
    /**
     * Returns the saved position of the given object.
     *
     * @param position the saved position of the given object will be stored here
     * @returns true iff a position had been stored
     */
    [Pure]
    public bool GetSavedPosition(Guid guid, out Vector3 position)
    {
        return _objectPositions.TryGetValue(guid.guid, out position);
    }
    
    /**
     * Save the position value of an object
     */
    public void SavePosition(Guid guid)
    {
        _objectPositions[guid.guid] = guid.transform.position;
    }
    
    /**
     * Remove the position value of an object, if it had been saved
     *
     * @returns true iff there had been a position stored for the given object
     */
    public bool RemovePosition(Guid guid)
    {
        return _objectPositions.Remove(guid.guid);
    }

    /**
     * Marks a key as collected by the players.
     */
    public void MarkKeyAsCollected(Key key)
    {
        _collectedKeys.Add(key.Guid.guid);
    }

    /**
     * True iff the given key has already been collected by the players.
     */
    [Pure]
    public bool HasKeyBeenCollected(Key key)
    {
        return _collectedKeys.Contains(key.Guid.guid);
    }
    
    /**
     * Marks an object as persistently destroyed, e.g. it shall not spawn again when reloading its scene.
     * See also `PersistentlyDestructible` behavior.
     */
    public void MarkAsDestroyed(PersistentlyDestructible obj)
    {
        _destroyedObjects.Add(obj.guid.guid);
    }

    /**
     * True iff the given object had been marked as previously destroyed
     */
    [Pure]
    public bool HasObjectBeenDestroyed(PersistentlyDestructible obj)
    {
        return _destroyedObjects.Contains(obj.guid.guid);
    }
}
