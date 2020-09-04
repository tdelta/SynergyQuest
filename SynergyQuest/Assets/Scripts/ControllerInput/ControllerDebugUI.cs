using System;
using TMPro;
using UnityEngine;

/**
 * Displays remote inputs from the ControllerInput class live as an UI element for debugging purposes.
 */
public class ControllerDebugUI : MonoBehaviour
{
    private ControllerInput _input;

    [SerializeField] private TextMeshProUGUI _connectionStatusLabel = default;
    
    [SerializeField] private TextMeshProUGUI _verticalValLabel = default;
    
    [SerializeField] private TextMeshProUGUI _horizontalValLabel = default;
    
    [SerializeField] private TextMeshProUGUI _attackValLabel = default;
    
    [SerializeField] private TextMeshProUGUI _pullValLabel = default;

    public void SetInput(ControllerInput input)
    {
        this._input = input;
        RegisterCallbacks();
        SetConnectionStatusMsg();
    }
    
    private void OnDisconnect(ControllerInput input)
    {
        SetConnectionStatusMsg();
    }

    private void OnReconnect(ControllerInput input)
    {
        SetConnectionStatusMsg();
    }

    public void OnSetColorButton()
    {
        _input.SetColor(PlayerColor.Red);
    }

    private void OnEnable()
    {
        if (_input != null)
        {
            RegisterCallbacks();
            SetConnectionStatusMsg();
        }
    }

    private void RegisterCallbacks()
    {
        _input.OnReconnect += OnReconnect;
        _input.OnDisconnect += OnDisconnect;
    }

    private void OnDisable()
    {
        _input.OnReconnect -= OnReconnect;
        _input.OnDisconnect -= OnDisconnect;
    }

    // Update is called once per frame
    void Update()
    {
        _attackValLabel.SetText(_input.GetButton(Button.Attack).ToString());
        _pullValLabel.SetText(_input.GetButton(Button.Pull).ToString());
        _verticalValLabel.SetText((Math.Truncate(_input.GetVertical() * 100)/100).ToString());
        _horizontalValLabel.SetText((Math.Truncate(_input.GetHorizontal() * 100)/100).ToString());
    }

    private void SetConnectionStatusMsg()
    {
        string connectionStatusMsg;
        switch (_input.ConnectionStatus)
        {
            case ConnectionStatus.Connected:
                connectionStatusMsg = "Connected";
                break;
            case ConnectionStatus.NotConnected:
                connectionStatusMsg = "Lost Connection";
                break;
            default:
                connectionStatusMsg = "Unkown State";
                break;
        }
        
        _connectionStatusLabel.SetText(connectionStatusMsg);
    }
}
