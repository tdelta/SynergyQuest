using System.Collections.Generic;
using UnityEngine;

/**
 * This singleton allows to set shared global state variables for all remote controllers.
 * For example, the current state of the game can be set (whether its currently in lobby mode or started, etc.)
 *
 * It also makes sure, to send the current state of these variables to newly connected controllers and to controllers
 * which disconnected, when they reconnect.
 *
 * Usage example:
 * ```
 * SharedControllerState.Instance.SetGameState(GameState.Lobby);
 * ```
 */
public class SharedControllerState: BehaviourSingleton<SharedControllerState>
{
    // The game starts in lobby mode
    public GameState GameState { get; private set; } = GameState.Lobby;
    
    private HashSet<MenuAction> _enabledMenuActions = new HashSet<MenuAction>()
    { // These actions are initially enabled 
        MenuAction.PauseGame,
        MenuAction.ShowMap
    };
    
    /**
     * Makes sure shared controller state is managed `OnAwake` in Unity by ensuring this singleton is instantiated.
     */
    [RuntimeInitializeOnLoadMethod]
    public static void EnsureInitialization()
    {
        var _ = SharedControllerState.Instance;
    }
    
    void OnEnable()
    {
        // Register all relevant callbacks, when this behavior is enabled
        ControllerServer.Instance.OnNewController += OnNewController;
        ControllerServer.Instance.GetInputs().ForEach( OnNewController );
    }

    private void OnDisable()
    {
        if (ControllerServer.Instance != null)
        {
            // Unregister all callbacks, when this behavior is disabled
            ControllerServer.Instance.OnNewController -= OnNewController;
            ControllerServer.Instance.GetInputs().ForEach(
                input =>
                {
                    input.OnReconnect -= OnReconnect;
                    input.OnMenuActionTriggered -= MenuActionTriggeredHandler;
                }
            );
        }
    }

    /**
     * Tells all connected controllers, in which state the game currently is.
     * Disconnected and new controllers will be informed, as soon as they connect.
     * 
     * For example, if the game is displaying the lobby and then starts, it should tell the controllers here
     * that the game is now in the state `GameState.Started`.
     */
    public void SetGameState(GameState gameState)
    {
        // Only send messages about updated state, if it actually changed
        if (gameState != this.GameState)
        {
            this.GameState = gameState;
            
            // Send new game state to all connected controllers
            ControllerServer.Instance.GetInputs().ForEach( input =>
            {
                // Only send game state, if the controller is connected.
                // Otherwise, the `OnReconnect` handler will send it.
                if (input.IsConnected())
                {
                    input.SetGameState(this.GameState);
                }
            });
        }
    }
    
    /**
     * Tells the controller, which menu actions are currently available in the game.
     * For example, when the game can be paused, MenuAction.PauseGame should be enabled.
     *
     * Example:
     * ```
     * SharedControllerState.Instance.EnableMenuActions(
     *     (MenuAction.PauseGame, true),
     *     (MenuAction.ResumeGame, false)
     * );
     * ```
     *
     * @param actions pairs of menu actions and a boolean which indicates, whether the action
     *                should be enabled
     */
    public void EnableMenuActions(params (MenuAction, bool)[] actions)
    {
        foreach (var (action, enabled) in actions)
        {
            if (enabled)
            {
                _enabledMenuActions.Add(action);
            }

            else
            {
                _enabledMenuActions.Remove(action);
            }
        }
        
        // Send updated menu actions to every controller
        ControllerServer.Instance.GetInputs().ForEach( input =>
        {
            // Only send actions, if the controller is connected.
            // Otherwise, the `OnReconnect` handler will send them.
            if (input.IsConnected())
            {
                input.SetEnabledMenuActions(_enabledMenuActions);
            }
        });
    }
    
    /**
     * This event is emitted when a certain menu action is selected on any controller.
     * E.g. if a controller wants to pause the game etc.
     */
    public event MenuActionTriggeredAction OnMenuActionTriggered;

    /**
     * Callback which is called when a new controller connects.
     */
    private void OnNewController(ControllerInput input)
    {
        // Register additional callbacks for this specific controller
        input.OnReconnect += OnReconnect;
        
        // We can handle a new connection like a reconnect
        OnReconnect(input);
    }

    /**
     * Callback which is called when a disconnected controller reconnects or a new controller connects.
     */
    private void OnReconnect(ControllerInput input)
    {
        // Send the state of all variables shared across controllers to the reconnected controller
        input.SetGameState(GameState);
        input.SetEnabledMenuActions(_enabledMenuActions);

        input.OnMenuActionTriggered += MenuActionTriggeredHandler;
    }

    private void MenuActionTriggeredHandler(MenuAction action)
    {
        OnMenuActionTriggered?.Invoke(action);
    }

    /**
     * Local debug inputs register here, so that their menu actions can be listened to by subscribers
     * of the `OnMenuActionTriggeredEvent`.
     *
     * They must call `UnregisterLocalDebugInput` when being destroyed.
     * See also the `LocalInput` class.
     */
    public void RegisterLocalDebugInput(Input input)
    {
        input.OnMenuActionTriggered += MenuActionTriggeredHandler;
    }
    
    /**
     * Must be called by local debug instances when they are being destroyed, if they called `RegisterLocalDebugInput`
     * before.
     */
    public void UnregisterLocalDebugInput(Input input)
    {
        input.OnMenuActionTriggered -= MenuActionTriggeredHandler;
    }
}
