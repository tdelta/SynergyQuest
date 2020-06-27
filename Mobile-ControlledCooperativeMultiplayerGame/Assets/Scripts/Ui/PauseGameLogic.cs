using UnityEngine;

/**
 * Allows to pause and resume the game.
 * It also displays an appropriate UI screen and informs the controllers about the game being paused.
 *
 * It retrieves a reference to a prefab of the UI from the `MenuPrefabSettings` scriptable object.
 *
 * Usage example:
 * ```
 * PauseGameLogic.Instance.Pause();
 * ```
 */
public class PauseGameLogic : BehaviourSingleton<PauseGameLogic>
{
    // Instance of the pause screen UI. Instantiated and set in the Awake() method.
    private PauseScreenUi _pauseScreenUi;

    // Stores the value of Time.timeScale before pausing, so that it can be restored when resuming
    private float _timeScaleBeforePause;
    private bool _isPaused = false;

    public bool IsPaused => _isPaused;

    void Awake()
    {
        // Load the scriptable object which stores references to menu UI prefabs
        var menuPrefabSettings = Resources.Load<MenuPrefabSettings>("MenuPrefabSettings");

        // Instantiate the pause screen UI
        _pauseScreenUi = Instantiate(menuPrefabSettings.pauseScreenUiPrefab);
        _pauseScreenUi.Init(this);
        // Keep it invisible, until it is needed
        _pauseScreenUi.SetVisible(false);
    }

    /**
     * Pause the game.
     * It also displays an appropriate UI screen and informs the controllers about the game being paused.
     */
    public void Pause()
    {
        if (!_isPaused)
        {
            _isPaused = true;
            
            // Store the value of Time.timeScale before pausing, so that it can be restored when resuming
            _timeScaleBeforePause = Time.timeScale;
            // Set Time.timeScale to zero, which effectively pauses the game by freezing time.
            Time.timeScale = 0.0f;

            _pauseScreenUi.SetVisible(true);
            
            // Inform the controllers about the game being paused
            // FIXME: We should have a MenuManager class that takes care of these things for all menus.
            SharedControllerState.Instance.SetGameState(GameState.Menu);
            SharedControllerState.Instance.EnableMenuActions(
                (MenuAction.ResumeGame, true),
                (MenuAction.PauseGame, false)
            );
        }
    }

    /**
     * Resume the game.
     * It also informs the controllers about the game being resumed.
     */
    public void Resume()
    {
        if (_isPaused)
        {
            _isPaused = false;
            
            // Make time flow again, like it did before pausing
            Time.timeScale = _timeScaleBeforePause;

            _pauseScreenUi.SetVisible(false);
            
            // Inform the controllers about the game being resumed
            // FIXME: We should have a MenuManager class that takes care of these things for all menus.
            SharedControllerState.Instance.SetGameState(GameState.Started);
            SharedControllerState.Instance.EnableMenuActions(
                (MenuAction.ResumeGame, false),
                (MenuAction.PauseGame, true)
            );
        }
    }
}
