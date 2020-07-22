using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class EndOfGameUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNamesText;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get all connected controllers
        var inputs = ControllerServer.Instance.GetInputs();
        
        // Display the names of all players
        var playerNames = inputs.Select(input => input.PlayerName);
        _playerNamesText.SetText(string.Join(" ", playerNames));
    }

    private void OnEnable()
    {
        // Allow each controller to quit the game by entering a menu state
        SharedControllerState.Instance.SetGameState(GameState.Menu);
        SharedControllerState.Instance.OnMenuActionTriggered += OnMenuActionTriggered;
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.QuitGame, true),
            (MenuAction.PauseGame, false)
        );
    }

    private void OnDisable()
    {
        // The controllers shall leave the menu state when this screen is closed 
        SharedControllerState.Instance.SetGameState(GameState.Started);
        SharedControllerState.Instance.OnMenuActionTriggered -= OnMenuActionTriggered;
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.QuitGame, false),
            (MenuAction.PauseGame, true)
        );
    }

    private void OnMenuActionTriggered(MenuAction action)
    {
        if (action == MenuAction.QuitGame)
        {
            SceneController.Instance.QuitGame();
        }
    }
}
