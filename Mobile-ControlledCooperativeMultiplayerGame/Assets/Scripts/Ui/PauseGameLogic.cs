using UnityEngine;

/**
 * Allows to pause and resume the game.
 * Usually you do not want to use this class directly, but call the Pause UI or InfoScreen UI instead.
 * (See `PauseScreenLauncher` and `InfoScreenLauncher`)
 *
 * Usage example:
 * ```
 * PauseGameLogic.Instance.Pause();
 * ```
 */
public class PauseGameLogic : Singleton<PauseGameLogic>
{
    // Stores the value of Time.timeScale before pausing, so that it can be restored when resuming
    private float _timeScaleBeforePause;
    private bool _isPaused = false;

    public bool IsPaused => _isPaused;

    /**
     * Pause the game by freezing time.
     * 
     * It also changes the available menu options and game state for remote controllers
     * (i.e. whether the pause button is available etc.)
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

            // Inform the controllers about the game being paused / in menu state
            SharedControllerState.Instance.SetGameState(GameState.Menu);
            SharedControllerState.Instance.EnableMenuActions(
                (MenuAction.PauseGame, false)
            );
        }
    }

    /**
     * Resume the game.
     * 
     * It also changes the available menu options and game state for remote controllers
     * (i.e. whether the pause button is available etc.)
     */
    public void Resume()
    {
        if (_isPaused)
        {
            _isPaused = false;
            
            // Make time flow again, like it did before pausing
            Time.timeScale = _timeScaleBeforePause;

            // Inform the controllers about the game being resumed
            SharedControllerState.Instance.SetGameState(GameState.Started);
            SharedControllerState.Instance.EnableMenuActions(
                (MenuAction.PauseGame, true)
            );
        }
    }
}
