using UnityEngine;
using UnityEngine.EventSystems;

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
    protected UiType UiInstance { get; private set; }

    /**
     * Caches the state of the game before entering a menu so that it can be restored when
     * closing the menu
     */
    private GameState _gameStateBeforeMenu;
    
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
            if (UiInstance != null)
            {
                Debug.LogError("Can not launch menu which is already active.");
            }

            else
            {
                // Make sure an event system is present, otherwise, buttons will not work
                if (Object.FindObjectOfType<EventSystem>() is null)
                {
                    // If none is present, create one
                    // ReSharper disable once ObjectCreationAsStatement
                    new GameObject(
                        "EventSystem",
                        typeof(EventSystem),
                        typeof(StandaloneInputModule)
                    );
                }
                
                // Instantiate UI prefab
                UiInstance = Object.Instantiate(GetUiPrefab());
                onUiInstantiated?.Invoke(UiInstance);
                
                // Pause the game
                PauseGameLogic.Instance.Pause();
                // Cache the state of the game before entering the menu
                _gameStateBeforeMenu = SharedControllerState.Instance.GameState;
                // Inform the controllers about the game being paused / in menu state
                SharedControllerState.Instance.SetGameState(GameState.Menu);
                SharedControllerState.Instance.EnableMenuActions(
                    (MenuAction.PauseGame, false)
                );

                // Allow the UI to receive menu actions from the controllers
                SharedControllerState.Instance.OnMenuActionTriggered += UiInstance.OnMenuActionTriggered;
                
                // More callbacks to inform about the state of the UI
                UiInstance.OnLaunch();
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
        if (UiInstance == null)
        {
            Debug.LogError("Can not close menu which has not been opened yet.");
        }

        else
        {
            // When being closed, the UI should no longer receive menu actions from the controllers
            SharedControllerState.Instance.OnMenuActionTriggered -= UiInstance.OnMenuActionTriggered;
            
            // Inform UI prefab instance that it is about to be destroyed
            UiInstance.OnClose();
            // Inform subclass that Ui is being closed
            OnUiClosed();
            
            // Destroy UI prefab instance
            Object.Destroy(UiInstance.gameObject);
            UiInstance = null;
            
            // Inform the controllers about the game being resumed by restoring the previous game state
            SharedControllerState.Instance.SetGameState(_gameStateBeforeMenu);
            SharedControllerState.Instance.EnableMenuActions(
                (MenuAction.PauseGame, true)
            );
            
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

    /**
     * Allows menus to react to menu actions triggered by controllers in addition to pressed UI buttons
     */
    void OnMenuActionTriggered(MenuAction action);
}