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
        
        // Allow each controller to quit the game
        foreach (var input in inputs)
        {
            input.OnMenuActionTriggered += OnMenuActionTriggered;
        }
        SharedControllerState.Instance.EnableMenuActions((MenuAction.QuitGame, true));
    }

    private void OnDestroy()
    {
        ControllerServer.Instance.GetInputs().ForEach(input => input.OnMenuActionTriggered -= this.OnMenuActionTriggered);
    }

    private void OnMenuActionTriggered(MenuAction action)
    {
        if (action == MenuAction.QuitGame)
        {
            SceneController.Instance.QuitGame();
        }
    }
}
