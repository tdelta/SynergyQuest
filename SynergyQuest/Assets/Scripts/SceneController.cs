// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

using System;
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
     * <summary>
     * Callback that will be invoked when a new scene has finished loading.
     * </summary>
     */
    public delegate void SceneFullyLoadedCallback();
    
    /**
     * <summary>
     * Callback that will be invoked when a new scene starts loading.
     * </summary>
     */
    public delegate void NewSceneLoadingCallback();

    public event NewSceneLoadingCallback OnNewSceneLoading;
    public event SceneFullyLoadedCallback OnSceneFullyLoaded;
    
    private SceneFullyLoadedCallback onSceneFullyLoadedCallback;
    /**
     * Which transition animation to play when changing scenes.
     */
    private TransitionType _transitionType = TransitionType.None;

    public bool IsLoadingScene { get; private set; } = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void EnsureInitialization()
    {
        var _ = Instance;
    }

    protected override void OnInstantiate()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChangedHandler;
        SceneManager.sceneLoaded += OnSceneFullyLoadedHandler;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChangedHandler;
        SceneManager.sceneLoaded -= OnSceneFullyLoadedHandler;
    }

    public void LoadMainMenu()
    {
        LoadSceneByName("MainMenuScene");
    }
    
    public void LoadPreLobbyInfo()
    {
        LoadSceneByName("PreLobbyInfo");
    }
    
    public void LoadExternalLibCredits()
    {
        LoadSceneByName("ExternalLibCredits");
    }
    
    public void LoadCredits()
    {
        LoadSceneByName("Credits");
    }
    
    public void LoadLicenseMenu()
    {
        LoadSceneByName("License");
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
     * <summary>
     * Loads any scene by its name. Note however that this bypasses passing any required parameters to scenes.
     * The scene must be added and enabled in the BuildSettings dialog.
     * </summary>
     * <param name="sceneName">
     *     name of the scene to load. May also be a relative path to a scene as shown in the BuildSettings window. (relative to the "Assets" folder and without file extension.
     * </param>
     * <param name="transitionType">
     *     the animation to play when changing scenes (optional)
     * </param>
     * <param name="callback">
     *     callback to invoke, when the new scene has finished loading (optional)
     * </param>
     */
    public void LoadSceneByName(string sceneName, TransitionType transitionType = TransitionType.None, SceneFullyLoadedCallback callback = null)
    {
        onSceneFullyLoadedCallback = callback;
        _transitionType = transitionType;
        
        // Play animation to transition out of the current scene
        TransitionController.Instance.TransitionOutOfScene(transitionType, () =>
        {
            // When the animation completed, load the new scene
            IsLoadingScene = true;
            
            SceneManager.LoadScene(sceneName);
        });
    }
    
    private void OnSceneFullyLoadedHandler(Scene scene, LoadSceneMode _)
    {
        if (scene.name == "LobbyMenu")
        {
            LobbyMenuUi lobbyMenu = GameObject.Find("LobbyMenuUi").GetComponent<LobbyMenuUi>();
            lobbyMenu.IPs = this._loadMenuIPs;
        }
        
        // Finish playing the transition animation
        TransitionController.Instance.TransitionIntoScene(_transitionType, () => { });

        IsLoadingScene = false;
        
        // If a callback has been set, invoke it now, since the new scene has finished loading
        onSceneFullyLoadedCallback?.Invoke();
        
        OnSceneFullyLoaded?.Invoke();
    }

    private void OnActiveSceneChangedHandler(Scene replacedScene, Scene nextScene)
    {
        OnNewSceneLoading?.Invoke();
    }
}
