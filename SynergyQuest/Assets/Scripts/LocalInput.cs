using System.Collections.Generic;
using UnityEngine;

/**
 * Implements input for a player character from the local keyboard.
 * 
 * See also `Input` interface.
 */
public class LocalInput: MonoBehaviour, Input
{
    /**
     * What keymap to use. Use different `LocalInput` instances with different layouts for multiple players
     */
    [SerializeField] private LocalKeymap keymap = default;
    /**
     * What color these controls shall report. It emulates the color assignment remote controllers undergo
     */
    [SerializeField] private PlayerColor color = default;

    /**
     * <summary>
     * What input mode these controls shall report. It emulates the input mode setting of remote controllers,
     * see also <see cref="InputMode"/>.
     * </summary>
     */
    public InputMode InputMode {
        get;
        set;
    } = InputMode.Normal;
    
    public event MenuActionTriggeredAction OnMenuActionTriggered;

    private void OnEnable()
    {
        SharedControllerState.Instance.RegisterLocalDebugInput(this);
    }
    
    private void OnDisable()
    {
        SharedControllerState.Instance.UnregisterLocalDebugInput(this);
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(keymap.PauseKey))
        {
            OnMenuActionTriggered?.Invoke(
                PauseScreenLauncher.Instance.IsPaused || InfoScreenLauncher.Instance.IsShowingInfoScreen?
                      MenuAction.ResumeGame
                    : MenuAction.PauseGame
            );
        }
        
        if (UnityEngine.Input.GetKeyDown(keymap.MapKey))
        {
            OnMenuActionTriggered?.Invoke(MenuAction.ShowMap);
        }
    }
    
    public void SetColor(PlayerColor color)
    {
        this.color = color;
    }
    
    public bool GetButton(Button button)
    {
        if (keymap.ButtonMappings.TryGetValue(button, out var keyCode))
        {
            return UnityEngine.Input.GetKey(keyCode);
        }

        return false;
    }

    public bool GetButtonDown(Button button)
    {
        if (keymap.ButtonMappings.TryGetValue(button, out var keyCode))
        {
            return UnityEngine.Input.GetKeyDown(keyCode);
        }

        return false;
    }
    
    public bool GetButtonUp(Button button)
    {
        if (keymap.ButtonMappings.TryGetValue(button, out var keyCode))
        {
            return UnityEngine.Input.GetKeyUp(keyCode);
        }

        return false;
    }

    public float GetVertical()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? 0 : UnityEngine.Input.GetAxis(keymap.VerticalAxis);
    }

    public float GetHorizontal()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? 0 : UnityEngine.Input.GetAxis(keymap.HorizontalAxis);
    }

    public float GetIMUOrientationVertical()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? UnityEngine.Input.GetAxis(keymap.VerticalAxis) : 0;
    }

    public float GetIMUOrientationHorizontal()
    {
        return UnityEngine.Input.GetKey(keymap.ImuOrientationModifier) ? UnityEngine.Input.GetAxis(keymap.HorizontalAxis) : 0;
    }

    /**
     * The built-in local controls always return the color they were initialized with.
     */
    public PlayerColor GetColor()
    {
        return color;
    }

    public void PlayVibrationFeedback(List<float> vibrationPattern)
    {
        // Vibration is not supported locally, we do nothing
    }

    /**
     * Keys on the keyboard cannot disappear. Do nothing
     */
    public void EnableButtons(params (Button, bool)[] buttonStates)
    { }

    /**
     * Keys on the keyboard cannot display a cooldown state. Do nothing.
     */
    public void SetCooldownButtons(params (Button, bool)[] buttonStates)
    { }

    /**
     * The keyboard cannot display information. Do nothing.
     */
    public void UpdatePlayerInfo(PlayerInfo info)
    { }
}