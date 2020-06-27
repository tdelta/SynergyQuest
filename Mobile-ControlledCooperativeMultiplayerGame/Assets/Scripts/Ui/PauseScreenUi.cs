using UnityEngine;

/**
 * Manages the UI which is displayed when the game is paused.
 * It does not handle the actual logic for pausing the game. See the `PauseGameLogic` class for that purpose
 */
public class PauseScreenUi : MonoBehaviour
{
    private PauseGameLogic _pauseGameLogic;

    public void Init(PauseGameLogic pauseGameLogic)
    {
        _pauseGameLogic = pauseGameLogic;
    }

    public void SetVisible(bool visibility)
    {
        gameObject.SetActive(visibility);
    }

    /**
     * Callback which is invoked when the resume button on the screen is pressed
     */
    public void OnResumePressed()
    {
        _pauseGameLogic.Resume();
    }
}
