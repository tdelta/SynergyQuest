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
        }
    }

    /**
     * Resume the game.
     */
    public void Resume()
    {
        if (_isPaused)
        {
            _isPaused = false;
            
            // Make time flow again, like it did before pausing
            Time.timeScale = _timeScaleBeforePause;
        }
    }
}
