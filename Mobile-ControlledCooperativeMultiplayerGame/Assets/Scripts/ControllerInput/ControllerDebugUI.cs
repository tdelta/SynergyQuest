using TMPro;
using UnityEngine;

public class ControllerDebugUI : MonoBehaviour
{
    [SerializeField] ControllerInput input;

    [SerializeField] TextMeshProUGUI verticalValLabel;
    
    [SerializeField] TextMeshProUGUI horizontalValLabel;
    
    [SerializeField] TextMeshProUGUI attackValLabel;
    
    [SerializeField] TextMeshProUGUI pullValLabel;

    // Update is called once per frame
    void Update()
    {
        attackValLabel.SetText(input.GetButton(Button.Attack).ToString());
        pullValLabel.SetText(input.GetButton(Button.Pull).ToString());
        verticalValLabel.SetText(input.Vertical().ToString());
        horizontalValLabel.SetText(input.Horizontal().ToString());
    }
}
