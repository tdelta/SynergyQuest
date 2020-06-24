
using System.Collections.Generic;
using UnityEngine;

/**
 * Implements input for a player character from the local keyboard.
 * 
 * See also `Input` interface.
 */
public class LocalInput: Input
{
    private LocalKeyboardLayout _mode;
    private PlayerColor _color;
    
    /**
     * Constructor
     * 
     * @param keyboardLayout what layout to use. Use different `LocalInput` instances with different layouts for multiple players
     * @param color          what color these controls shall report. It emulates the color assignment remote controllers undergo
     */
    public LocalInput(
        LocalKeyboardLayout keyboardLayout,
        PlayerColor color
    )
    {
        _mode = keyboardLayout;
        _color = color;
    }
    
    public bool GetButton(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKey(_mode.AttackKey());
            case Button.Pull:
                return UnityEngine.Input.GetKey(_mode.PullKey());
        }

        return false;
    }

    public bool GetButtonDown(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKeyDown(_mode.AttackKey());
            case Button.Pull:
                return UnityEngine.Input.GetKeyDown(_mode.PullKey());
        }

        return false;
    }
    
    public bool GetButtonUp(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKeyUp(_mode.AttackKey());
            case Button.Pull:
                return UnityEngine.Input.GetKeyUp(_mode.PullKey());
        }

        return false;
    }

    public float GetVertical()
    {
        return UnityEngine.Input.GetAxis(_mode.VerticalAxis());
    }

    public float GetHorizontal()
    {
        return UnityEngine.Input.GetAxis(_mode.HorizontalAxis());
    }
    
    /**
     * The built-in local controls always return the color they were initialized with.
     */
    public PlayerColor GetColor()
    {
        return _color;
    }

    public void PlayVibrationFeedback(List<float> vibrationPattern)
    {
        // Vibration is not supported locally, we do nothing
    }
}

/**
 * The local keyboard allows for up to two players.
 * This enum encodes the different layouts that can be used.
 */
public enum LocalKeyboardLayout
{
    /**
     * * WASD movement
     * * Space attack
     * * c pull
     */
    WASD,
    /**
     * * arrow key movement
     * * j attack
     * * k pull
     */
    Arrow
}

/**
 * These extension methods of `LocalKeyboardLayout` contain the mapping from more "abstract" actions (attack/pull/...)
 * to concrete keys for the different layouts.
 */
static class LocalControlModeMethods
{
    public static string HorizontalAxis(this LocalKeyboardLayout mode)
    {
        switch (mode)
        {
            case LocalKeyboardLayout.WASD:
                return "Horizontal";
            case LocalKeyboardLayout.Arrow:
                return "HorizontalArrows";
        }

        return "Horizontal";
    }

    public static string VerticalAxis(this LocalKeyboardLayout mode)
    {
        switch (mode)
        {
            case LocalKeyboardLayout.WASD:
                return "Vertical";
            case LocalKeyboardLayout.Arrow:
                return "VerticalArrows";
        }

        return "Vertical";
    }
    
    public static KeyCode AttackKey(this LocalKeyboardLayout mode)
    {
        switch (mode)
        {
            case LocalKeyboardLayout.WASD:
                return KeyCode.Space;
            case LocalKeyboardLayout.Arrow:
                return KeyCode.J;
        }

        return KeyCode.Space;
    }
    
    public static KeyCode PullKey(this LocalKeyboardLayout mode)
    {
        switch (mode)
        {
            case LocalKeyboardLayout.WASD:
                return KeyCode.C;
            case LocalKeyboardLayout.Arrow:
                return KeyCode.K;
        }

        return KeyCode.C;
    }
}
