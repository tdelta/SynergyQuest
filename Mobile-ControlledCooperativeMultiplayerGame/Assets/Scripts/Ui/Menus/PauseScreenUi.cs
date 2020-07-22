using UnityEngine;

/**
 * Controls the UI of the pause screen.
 * To actually launch the pause screen and pause the game, see the `PauseScreenLauncher` singleton.
 *
 * See also the `MenuUi` interface for a description of its methods.
 */
public class PauseScreenUi : MonoBehaviour, MenuUi
{
    /**
     * Callback which is invoked when the resume button on the screen is pressed
     */
    public void OnResumePressed()
    {
        PauseScreenLauncher.Instance.Close();
    }

    public void OnQuitGamePressed()
    {
        PauseScreenLauncher.Instance.Close();
        QuitGameScreenLauncher.Instance.Launch();
    }

    public void OnLaunch()
    {
        // When the pause screen UI is opened, give remote controllers the capability to close the pause screen and resume the game
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, true),
            (MenuAction.QuitGame, true)
        );
    }
    
    public void OnClose()
    {
        // When the pause screen UI is closed and destroyed, remove the "Resume" capability from remote controllers
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, false),
            (MenuAction.QuitGame, false)
        );
    }

    public void OnMenuActionTriggered(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.ResumeGame:
                OnResumePressed();
                break;
            case MenuAction.QuitGame:
                OnQuitGamePressed();
                break;
        }
    }
}
