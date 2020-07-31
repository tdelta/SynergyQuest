using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class ThrowableItem : Item
{
    /** Implements the effect of the item when used
     *
     * @param player: The player that uses the item
     */
    public abstract Throwable PickUp(PlayerController player);
    
    /** Implements the effect of the item when used
     *
     * @param player: The player that uses the item
     */
    protected abstract void Throw(PlayerController player, Throwable item);

    // Instanciated when the player picks up the item
    private Throwable _currentInstance = null;
    
    // Item was ready in the last update
    private bool _wasReady = false;

    public override void Update()
    {
        if (ReferenceEquals(_player, null))
            return;
        if (Ready()){
            if (!_wasReady){
                _player.EnableGameAction(GetButton());
                _wasReady = true;
            }
            if (ReferenceEquals(_currentInstance, null) && _player.Input.GetButtonDown(GetButton())){
                _currentInstance = PickUp(_player);
            }
        }
        else if (ReferenceEquals(_currentInstance, null)){
            _player.DisableGameAction(GetButton());
            _wasReady = false;
        }
        else if (_player.Input.GetButtonUp(GetButton())){
            Throw(_player, _currentInstance);
            _currentInstance = null;
        }
    }
}
