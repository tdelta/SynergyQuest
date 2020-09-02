using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

/**
 * Manages the LobbyMenu scene which requires players join before the game can be started.
 * It displays the address of the website which serves the controller client software.
 * It also displays this address as a QR code.
 *
 * Since there can be multiple local network addresses, at first it only displays one of them.
 * After a few seconds the users get the option to show all addresses.
 */
public class LobbyMenuUi : MonoBehaviour
{
    [SerializeField] private Canvas _canvas = default;
    [SerializeField] private Image _qrCodeImage = default;
    [SerializeField] private TextMeshProUGUI _addressValLabel = default;
    [SerializeField] private TextMeshProUGUI _joinedPlayersValLabel = default;
    [SerializeField] private TextMeshProUGUI _statusValLabel = default;
    [SerializeField] private UnityEngine.UI.Button _notWorkingButton = default;
    
    // minimum number of players to start the game
    [SerializeField] private int _minNumPlayers = 2;

    // Local IPs which are used to build the displayed addresses.
    // This field is set as a parameter by `SceneController` when loading this scene.
    public List<string> IPs;

    // if true, all local addresses are shown. See also `OnNotWorkingButton`
    private bool _displayAllAddresses = false;
    
    // color to be assigned to the next new player
    private PlayerColor _nextPlayerColor = PlayerColor.Red;

    // Start is called before the first frame update
    void Start()
    {
        // If IP addresses have been passed to this scene then we can continue
        if (IPs != null && IPs.Any())
        {
            // For every already connected controller, bind to its events
            ControllerServer.Instance.GetInputs().ForEach(InitializeInput);
            // And register a callback, should new controllers connect
            ControllerServer.Instance.OnNewController += OnNewController;
            // Allow controllers to display the "Start Game" menu action, if the game can already be started
            SetStartGameAction(CanStartGame());
            
            // Update the UI
            DisplayAddresses();
            DisplayStatus();
            
            // We have a button which lets users display all addresses, but by default it is invisible
            _notWorkingButton.gameObject.SetActive(false);
            if (IPs.Count() > 1)
            {
                // if there is more than 1 address, display the button after a few seconds
                StartCoroutine("MakeNotWorkingButtonVisible");
            }
        }

        // Otherwise we first must load the network setup scene
        else
        {
            SceneController.Instance.LoadNetworkSetup();
        }
    }

    private void Update()
    {
        // Allow to quit game in lobby with esc key
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.QuitGame();
        }
    }

    /**
     * Callback which is called by the "Not Working?" button in the scene.
     * It displays all local addresses on the screen
     */
    public void OnNotWorkingButton()
    {
        _displayAllAddresses = true;
        DisplayAddresses();
    }

    /**
     * Coroutine, which makes the "Not Working?" button visible after a few seconds.
     */
    private IEnumerator MakeNotWorkingButtonVisible()
    {
        yield return new WaitForSeconds(5.0f);
        _notWorkingButton.gameObject.SetActive(true);
    }

    /**
     * Binds callbacks of a new controller and assigns it a color.
     */
    private void InitializeInput(ControllerInput input)
    {
        BindInput(input);
        
        input.SetColor(_nextPlayerColor);
        _nextPlayerColor = _nextPlayerColor.NextColor();
    }

    /**
     * Binds callbacks to an input object to this object.
     */
    private void BindInput(ControllerInput input)
    {
        input.OnMenuActionTriggered += OnMenuActionTriggered;
    }

    /**
     * Unbinds callbacks of an input object to this object
     */
    private void UnbindInput(ControllerInput input)
    {
        input.OnMenuActionTriggered -= OnMenuActionTriggered;
    }
    
    /**
     * Enables the "Start Game" menu option on all connected controllers
     */
    private void SetStartGameAction(bool enable)
    {
        SharedControllerState.Instance.EnableMenuActions((MenuAction.StartGame, enable));
    }


    /**
     * Callback which is called by `ControllerServer` if a new controller connected.
     * It updates the UI and menu actions available on controllers, depending on how ready the game is to be started.
     */
    private void OnNewController(ControllerInput input)
    {
        InitializeInput(input);
        DisplayStatus();
        SetStartGameAction(CanStartGame());
    }

    /**
     * Callback which is called if a controller triggered a menu action.
     * If its the "Start Game" action, the game will be startet (i.e. the next scene is loaded)
     */
    private void OnMenuActionTriggered(MenuAction action)
    {
        if (action == MenuAction.StartGame && CanStartGame())
        {
            var inputs = ControllerServer.Instance.GetInputs();
            
            foreach (var input in inputs)
            {
                UnbindInput(input);
            }
            SharedControllerState.Instance.EnableMenuActions((MenuAction.StartGame, false));
            SharedControllerState.Instance.SetGameState(GameState.Started);

            DungeonLayout.Instance.LoadDungeon(
                StartupSettings.Instance.InitialDungeonLayoutFile,
                ControllerServer.Instance.GetInputs().Count
            );
        }
    }

    /**
     * Update displayed information besided the available local addresses
     */
    private void DisplayStatus()
    {
        _joinedPlayersValLabel.SetText(BuildJoinedPlayersString());
        _statusValLabel.SetText(BuildStatusString());
    }

    /**
     * Displays the local address(es) of the web page where the controller client software is served.
     * It also displays one of the addresses as QR code.
     *
     * Note, that only one address is listet, until `_displayAllAddresses` is set to true.
     * The field is set, if the "Not Working?" button in the scene is pressed.
     */
    private void DisplayAddresses()
    {
        // Derive address strings for the web server from our IPs
        var addresses = IPs.Select(BuildAddress).ToList();
        
        string addressesText;
        if (!_displayAllAddresses)
        {
            addressesText = addresses.First();
        }

        else
        {
            var orText = String.Join(" or ", addresses);
            addressesText = $"Try all of these addresses: {orText}";
        }
            
        // Display the addresses as text
        _addressValLabel.SetText(addressesText);
        
        // Create an QR code which encodes the address:
        // 1. Determine the resolution of the UI element which will display the QR code:
        var sizeDelta = _qrCodeImage.GetComponent<RectTransform>().sizeDelta;
        var canvasScale = _canvas.transform.localScale;
        var widthPixels = (int) (sizeDelta.x * canvasScale.x);
        var heightPixels = (int) (sizeDelta.y * canvasScale.y);
        
        // 2. Compute the QR code
        var qrCodeSprite = BuildQRCode(addresses.First(), widthPixels, heightPixels);

        // 3. Display it
        _qrCodeImage.sprite = qrCodeSprite;
    }

    /**
     * Retrieve the number of players which have yet connected.
     */
    private int GetNumConnectedPlayers()
    {
        return ControllerServer.Instance.GetInputs().Count();
    }

    /**
     * Returns true iff the game can be startet, that is, iff at least `_minNumPlayers` connected.
     */
    private bool CanStartGame()
    {
        return GetNumConnectedPlayers() >= _minNumPlayers;
    }

    /**
     * Renders the list of connected player names as a string.
     */
    private string BuildJoinedPlayersString()
    {
        var playerNames = ControllerServer.Instance.GetInputs().Select(input => input.PlayerName).ToList();
        if (playerNames.Any())
        {
            var playerList = String.Join(", ", playerNames);
            return $"Joined Players: {playerList}";
        }

        else
        {
            return "No one connected yet.";
        }
    }

    /**
     * Renders information as a string, whether the game can be startet yet or alternatively, how many players still
     * need to join so that it can start.
     */
    private string BuildStatusString()
    {
        if (CanStartGame())
        {
            return "Use the phone controls to start the game!";
        }

        else
        {
            var requiredAdditionalPlayers = _minNumPlayers - GetNumConnectedPlayers();
            string playersDeclination;
            if (requiredAdditionalPlayers > 1)
            {
                playersDeclination = "players";
            }

            else
            {
                playersDeclination = "player";
            }
            
            return $"You need at least {requiredAdditionalPlayers} more {playersDeclination} to start the game.";
        }
    }

    /**
     * Given a IP address, return the address where the web page resides which serves the controller client software.
     */
    private string BuildAddress(string ip)
    {
        return $"{ControllerServer.Instance.UsedProtocol}://{ip}:{ControllerServer.Instance.UsedPort}";
    }
    
    /**
     * Generates an QR code of a web address using the ZXing.Net library.
     * The code has been adapted from their usage example: https://github.com/micjahn/ZXing.Net/blob/master/Clients/UnityDemo/Assets/BarcodeCam.cs
     *
     * @param address web address we want to encode
     * @param width   width of the texture that shall be generated
     * @param height  height of the texture that shall be generated
     *
     * @returns A sprite. It contains a qr code that encodes "http://{address}". 
     */
    private Sprite BuildQRCode(string url, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 1
            },
        };
        
        var colorArray = writer.Write(url);
        
        var tex = new Texture2D(width, height);
        tex.SetPixels32(colorArray);
        tex.Apply();
        
        var sprite = Sprite.Create(
            tex,
            new Rect(0.0f, 0.0f, tex.width, tex.height), 
            new Vector2	(0.5f, 0.5f)
        );

        return sprite;
    }
}
