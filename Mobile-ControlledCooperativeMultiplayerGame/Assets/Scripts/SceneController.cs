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
public class SceneController : BehaviourSingleton<SceneController>
{
    // Some scenes want some data passed to them on load.
    // Since this can not be done directly, we use these fields to cache the date in between loads:
    
    // The LobbyMenu scene must be passed a list of IP addresses
    private List<string> _loadMenuIPs;

    /**
     * Callback that will be invoked when a new scene has finished loading.
     */
    public delegate void SceneLoadedCallback();
    private SceneLoadedCallback _onSceneLoadedCallback;
    /**
     * Which transition animation to play when changing scenes.
     */
    private TransitionType _transitionType = TransitionType.None;

    protected override void OnInstantiate()
    {
        // call the OnSceneLoaded method of this object, as soon as a level is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadNetworkSetup()
    {
        LoadSceneByName("NetworkSetup");
    }

    public void LoadLobbyMenu(List<string> ips)
    {
        LoadSceneByName("LobbyMenu");

        // cache the IPs, so they can be set by OnSceneLoaded as soon as the LobbyMenu scene has been loaded.
        _loadMenuIPs = ips;
    }

    public void LoadTestRoom()
    {
        LoadSceneByName("TestRoom");
    }

    public void LoadEndOfGame()
    {
        LoadSceneByName("EndOfGame");
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
     *
     * @param sceneName      name of the scene to load
     * @param transitionType the animation to play when changing scenes (optional)
     * @param callback       callback to invoke, when the new scene has finished loading (optional)
     */
    public void LoadSceneByName(string sceneName, TransitionType transitionType = TransitionType.None, SceneLoadedCallback callback = null)
    {
        _onSceneLoadedCallback = callback;
        _transitionType = transitionType;
        
        // Play animation to transition out of the current scene
        TransitionController.Instance.TransitionOutOfScene(transitionType, () =>
        {
            // When the animation completed, load the new scene
            SceneManager.LoadScene(sceneName);
        });
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (scene.name == "LobbyMenu")
        {
            LobbyMenuUi lobbyMenu = GameObject.Find("LobbyMenuUi").GetComponent<LobbyMenuUi>();
            lobbyMenu.IPs = this._loadMenuIPs;
        }
        
        // Finish playing the transition animation
        TransitionController.Instance.TransitionIntoScene(_transitionType, () => { });
        
        // If a callback has been set, invoke it now, since the new scene has finished loading
        _onSceneLoadedCallback?.Invoke();
    }
}
