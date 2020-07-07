using UnityEngine;

/**
 * Base class for singletons which control menu UI.
 *
 * It takes care of tasks common to all menu screens, e.g.
 * - Pausing the game
 * - Sending the refreshed game state to remote controllers
 * - Instantiating and destroying the menu UI
 *
 * See also for example the subclasses `PauseScreenLauncher` and `InfoScreenLauncher`
 *
 * The first type parameter must be the subclass itself.
 * The second type parameter must be the UI behavior.
 */
public abstract class MenuLauncher<LauncherType, UiType>: Singleton<LauncherType> 
    where LauncherType: new()
    where UiType: MonoBehaviour, MenuUi
{
    // We store an instance of the menu UI here, if it is active
    private UiType _uiInstance;
    protected UiType UiInstance => _uiInstance;
    
    /**
     * Subclasses must provide a prefab of the menu UI by implementing this method.
     */
    protected abstract UiType GetUiPrefab();
   
    protected delegate void OnUiInstantiatedAction(UiType ui);
    
    /**
     * Can be used by subclasses to launch the UI and perform some other initialization steps as described in the class
     * description.
     *
     * It will have no effect, if the game is already currently paused, for example by some other menu.
     * It will also have no effect if this menu is already active.
     *
     * @param onUiInstantiated callback which is invoked as soon as the UI prefab has been instantiated.
     *                         This can be used by subclasses to perform furter initialization steps on the prefab
     *                         instance.
     */
    protected void Launch(OnUiInstantiatedAction onUiInstantiated = null)
    {
        if (PauseGameLogic.Instance.IsPaused)
        {
            Debug.Log("Can not launch menu when game is already paused.");
        }
        
        else {
            if (_uiInstance != null)
            {
                Debug.LogError("Can not launch menu which is already active.");
            }

            else
            {
                // Instantiate UI prefab
                _uiInstance = Object.Instantiate(GetUiPrefab());
                onUiInstantiated?.Invoke(_uiInstance);
                
                // Pause the game
                PauseGameLogic.Instance.Pause();
               
                // More callbacks to inform about the state of the UI
                _uiInstance.OnLaunch();
                OnUiLaunched();
            }
        }
    }

    /**
     * Destroys the menu UI, unpauses the game and sets state on controllers accordingly.
     *
     * It will have no effect, if the menu is not currently active.
     */
    public void Close()
    {
        if (_uiInstance == null)
        {
            Debug.LogError("Can not close menu which has not been opened yet.");
        }

        else
        {
            // Inform UI prefab instance that it is about to be destroyed
            _uiInstance.OnClose();
            // Inform subclass that Ui is being closed
            OnUiClosed();
            
            // Destroy UI prefab instance
            Object.Destroy(_uiInstance.gameObject);
            _uiInstance = null;
            
            // Unpause the game
            PauseGameLogic.Instance.Resume();
        }
    }

    /**
     * Called as soon as the menu has been fully activated.
     */
    protected virtual void OnUiLaunched()
    { }
   
    /**
     * Called as soon as the menu has been fully destroyed.
     */
    protected virtual void OnUiClosed()
    { }
}

/**
 * All menu UI behaviors must implement this interface,
 * see `MenuLauncher` class.
 */
public interface MenuUi
{
    /**
     * Called as soon as the menu has been fully activated.
     */
    void OnLaunch();
    
    /**
     * Called before the menu is closed and destroyed.
     */
    void OnClose();
}