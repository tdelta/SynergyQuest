using UnityEngine;

/**
 * Controls the UI of a screen which offers the player to close the game or abort.
 * To actually launch this screen, see the `QuitGameScreenLauncher` singleton.
 *
 * See also the `MenuUi` interface for a description of its methods.
 */
public class QuitGameScreenUi : MonoBehaviour, MenuUi
{
    /**
     * Callback when the player decides to quit the game
     */
    public void OnYesPressed()
    {
        QuitGameScreenLauncher.Instance.Close();
        SceneController.Instance.QuitGame();
    }

    /**
     * Callback when the player decides not to quit the game
     */
    public void OnNoPressed()
    {
        QuitGameScreenLauncher.Instance.Close();
        PauseScreenLauncher.Instance.Launch();
    }

    public void OnLaunch()
    {
        // When this screen is opened, give remote controllers the capability to enter one of the menu actions
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.Yes, true),
            (MenuAction.No, true)
        );
    }
    
    public void OnClose()
    {
        // When this screen is closed and destroyed, remove the menu actions from remote controllers
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.Yes, false),
            (MenuAction.No, false)
        );
    }
    
    public void OnMenuActionTriggered(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.Yes:
                OnYesPressed();
                break;
            case MenuAction.No:
                OnNoPressed();
                break;
        }
    }
}
