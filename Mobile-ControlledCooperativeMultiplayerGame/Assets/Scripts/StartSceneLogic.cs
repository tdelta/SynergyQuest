using UnityEngine;

/**
 * The "StartScene" scene will be the first scene of the game. This class implements its game logic.
 * Currently, it only loads the "NetworkSetup" scene.
 */
public class StartSceneLogic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneController.Instance.LoadNetworkSetup();
    }
}
