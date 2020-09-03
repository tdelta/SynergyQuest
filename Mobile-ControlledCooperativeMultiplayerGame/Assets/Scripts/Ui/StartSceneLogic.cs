using UnityEngine;

/**
 * <summary>
 * The <c>StartScene</c> scene will be the first scene of the game. This class implements its game logic.
 * Currently, it only loads the <c>MainMenuScene</c>
 * </summary>
 */
public class StartSceneLogic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneController.Instance.LoadMainMenu();
    }
}
