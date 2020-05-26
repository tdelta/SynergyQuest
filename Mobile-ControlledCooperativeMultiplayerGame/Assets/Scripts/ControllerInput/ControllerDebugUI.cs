using System;
using TMPro;
using UnityEngine;

/**
 * Displays remote inputs from the ControllerInput class
 * live as an UI element for debugging purposes.
 */
public class ControllerDebugUI : MonoBehaviour
{
    [SerializeField] private ControllerInput input;

    [SerializeField] TextMeshProUGUI verticalValLabel;
    
    [SerializeField] TextMeshProUGUI horizontalValLabel;
    
    [SerializeField] TextMeshProUGUI attackValLabel;
    
    [SerializeField] TextMeshProUGUI pullValLabel;

    // Update is called once per frame
    void Update()
    {
        attackValLabel.SetText(input.GetButton(Button.Attack).ToString());
        pullValLabel.SetText(input.GetButton(Button.Pull).ToString());
        verticalValLabel.SetText((Math.Truncate(input.Vertical() * 100)/100).ToString());
        horizontalValLabel.SetText((Math.Truncate(input.Horizontal() * 100)/100).ToString());
    }
}
