using UnityEngine;

/**
 * <summary>
 * Controls the UI of the <c>PreLobbyInfo</c> scene. It simply allows to switch to the network setup scene after
 * pressing a button.
 * </summary>
 */
public class PreLobbyInfo : MonoBehaviour
{
    public void OnContinuePressed()
    {
        SceneController.Instance.LoadNetworkSetup();
    }
}
