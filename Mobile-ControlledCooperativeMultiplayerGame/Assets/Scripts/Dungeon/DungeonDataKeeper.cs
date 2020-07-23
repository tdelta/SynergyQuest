using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
}
