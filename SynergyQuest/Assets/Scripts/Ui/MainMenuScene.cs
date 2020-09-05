using UnityEngine;

/**
 * <summary>
 * Controls the UI of the main menu.
 * That is, it allows to switch to other scenes when buttons are pressed.
 * </summary>
 */
public class MainMenuScene : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        SceneController.Instance.LoadPreLobbyInfo();
    }
    
    public void OnCreditsButtonPressed()
    {
        SceneController.Instance.LoadCredits();
    }
}
