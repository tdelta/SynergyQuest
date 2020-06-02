using System;
using TMPro;
using UnityEngine;

/**
 * Displays remote inputs from the ControllerInput class
 * live as an UI element for debugging purposes.
 */
public class ControllerDebugUI : MonoBehaviour
{
    [SerializeField] private ControllerInput _input;

    [SerializeField] private TextMeshProUGUI _verticalValLabel;
    
    [SerializeField] private TextMeshProUGUI _horizontalValLabel;
    
    [SerializeField] private TextMeshProUGUI _attackValLabel;
    
    [SerializeField] private TextMeshProUGUI _pullValLabel;

    // Update is called once per frame
    void Update()
    {
        _attackValLabel.SetText(_input.GetButton(Button.Attack).ToString());
        _pullValLabel.SetText(_input.GetButton(Button.Pull).ToString());
        _verticalValLabel.SetText((Math.Truncate(_input.Vertical() * 100)/100).ToString());
        _horizontalValLabel.SetText((Math.Truncate(_input.Horizontal() * 100)/100).ToString());
    }
}
