using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * Encodes the keys with which the game can be controlled when using a keyboard
 * instead of a phone controller.
 *
 * To be used as a parameter to the `LocalInput` behavior.
 */
[CreateAssetMenu(fileName = "LocalKeymap", menuName = "ScriptableObjects/LocalKeymap")]
public class LocalKeymap : ScriptableObject
{
    [SerializeField] private string horizontalAxis = "Horizontal";
    public string HorizontalAxis => horizontalAxis;

    [SerializeField]
    private string verticalAxis = "Vertical";
    public string VerticalAxis => verticalAxis;

    /**
     * <summary>
     * If this modifier key is pressed . The vertical and horizontal axis input is used to emulate 3d orientation input.
     * </summary>
     * <seealso cref="ControllerInput.GetIMUOrientationVertical"/>
     * <seealso cref="ControllerInput.GetIMUOrientationHorizontal"/>
     */
    [SerializeField] private KeyCode imuOrientationModifier = KeyCode.LeftShift;
    public KeyCode ImuOrientationModifier => imuOrientationModifier;

    [SerializeField]
    private ButtonMapping[] buttonMappings =
    {
        new ButtonMapping(Button.Attack, KeyCode.Space),
        new ButtonMapping(Button.Pull, KeyCode.C),
        new ButtonMapping(Button.Carry, KeyCode.V),
        new ButtonMapping(Button.Press, KeyCode.Y),
        new ButtonMapping(Button.Throw, KeyCode.T),
        new ButtonMapping(Button.Read, KeyCode.Q),
        new ButtonMapping(Button.Open, KeyCode.E),
        new ButtonMapping(Button.UseBomb, KeyCode.B),
        new ButtonMapping(Button.Exit, KeyCode.Less),
    };

    private Dictionary<Button, KeyCode> _buttonMappingDictCache;
    public Dictionary<Button, KeyCode> ButtonMappings {
        get
        {
            // We create a dictionary for faster lookup of keys on first access.
            // (We have to use an array to store the mappings in the first place,
            //  since the unity inspector can not handle dictionaries)
            if (_buttonMappingDictCache is null)
            {
                _buttonMappingDictCache = new Dictionary<Button, KeyCode>();
                foreach (var buttonMapping in buttonMappings)
                {
                    _buttonMappingDictCache[buttonMapping.button] = buttonMapping.keyCode;
                }
            }

            return _buttonMappingDictCache;
        }
    }

    [SerializeField] private KeyCode pauseKey = KeyCode.R;
    public KeyCode PauseKey => pauseKey;
    
    [SerializeField] private KeyCode mapKey = KeyCode.Alpha1;
    public KeyCode MapKey => mapKey;
}

[Serializable]
public class ButtonMapping
{
    public Button button;
    public KeyCode keyCode;

    public ButtonMapping(Button button, KeyCode keyCode)
    {
        this.button = button;
        this.keyCode = keyCode;
    }
}