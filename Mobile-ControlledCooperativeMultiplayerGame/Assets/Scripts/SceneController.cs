using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Allows to load the different scenes of the game.
 * It is a singleton which survives level loads and whose instance can be accessed by the static `Instance`
 * property.
 *
 * For more info on the usage of such singleton classes, see the `Singleton` base class.
 */
public class SceneController : Singleton<SceneController>
{
    // Some scenes want some data passed to them on load.
    // Since this can not be done directly, we use these fields to cache the date in between loads:
    
    // The LobbyMenu scene must be passed a list of IP addresses
    private List<string> _loadMenuIPs;

    protected override void OnInstantiate()
    {
        // call the OnSceneLoaded method of this object, as soon as a level is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadNetworkSetup()
    {
        SceneManager.LoadScene("NetworkSetup");
    }

    public void LoadLobbyMenu(List<string> ips)
    {
        SceneManager.LoadScene("LobbyMenu");

        // cache the IPs, so they can be set by OnSceneLoaded as soon as the LobbyMenu scene has been loaded.
        _loadMenuIPs = ips;
    }

    public void LoadTestRoom()
    {
        SceneManager.LoadScene("TestRoom");
    }

    public void LoadEndOfGame()
    {
        SceneManager.LoadScene("EndOfGame");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }

    /**
     * Loads any scene by its name. Note however that this bypasses passing any required parameters to scenes.
     */
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (scene.name == "LobbyMenu")
        {
            LobbyMenuUi lobbyMenu = GameObject.Find("LobbyMenuUi").GetComponent<LobbyMenuUi>();
            lobbyMenu.IPs = this._loadMenuIPs;
        }
    }
}
