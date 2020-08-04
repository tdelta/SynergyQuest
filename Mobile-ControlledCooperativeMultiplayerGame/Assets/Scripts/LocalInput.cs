using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    [SerializeField] private LocalKeymap keymap;
    /**
     * What color these controls shall report. It emulates the color assignment remote controllers undergo
     */
    [SerializeField] private PlayerColor color;
    
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
        if (UnityEngine.Input.GetKeyDown(keymap.PauseKey()))
        {
            OnMenuActionTriggered?.Invoke(
                PauseScreenLauncher.Instance.IsPaused || InfoScreenLauncher.Instance.IsShowingInfoScreen?
                      MenuAction.ResumeGame
                    : MenuAction.PauseGame
            );
        }
    }
    
    public void SetKeymap(LocalKeymap keymap)
    {
        this.keymap = keymap;
    }
    
    public void SetColor(PlayerColor color)
    {
        this.color = color;
    }
    
    public bool GetButton(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKey(keymap.AttackKey());
            case Button.Pull:
                return UnityEngine.Input.GetKey(keymap.PullKey());
            case Button.Carry:
                return UnityEngine.Input.GetKey(keymap.CarryKey());
            case Button.Throw:
                return UnityEngine.Input.GetKey(keymap.ThrowKey());
            case Button.UseBomb:
                return UnityEngine.Input.GetKey(keymap.BombKey());

        }

        return false;
    }

    public bool GetButtonDown(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKeyDown(keymap.AttackKey());
            case Button.Pull:
                return UnityEngine.Input.GetKeyDown(keymap.PullKey());
            case Button.Carry:
                return UnityEngine.Input.GetKeyDown(keymap.CarryKey());
            case Button.Throw:
                return UnityEngine.Input.GetKeyDown(keymap.ThrowKey());
            case Button.UseBomb:
                return UnityEngine.Input.GetKeyDown(keymap.BombKey());
        }

        return false;
    }
    
    public bool GetButtonUp(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKeyUp(keymap.AttackKey());
            case Button.Pull:
                return UnityEngine.Input.GetKeyUp(keymap.PullKey());
            case Button.Carry:
                return UnityEngine.Input.GetKeyUp(keymap.CarryKey());
            case Button.Throw:
                return UnityEngine.Input.GetKeyUp(keymap.ThrowKey());
            case Button.UseBomb:
                return UnityEngine.Input.GetKeyUp(keymap.BombKey());
        }

        return false;
    }

    public float GetVertical()
    {
        return UnityEngine.Input.GetAxis(keymap.VerticalAxis());
    }

    public float GetHorizontal()
    {
        return UnityEngine.Input.GetAxis(keymap.HorizontalAxis());
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
}

/**
 * The local keyboard allows for up to two players.
 * This enum encodes the different layouts that can be used.
 */
public enum LocalKeymap
{
    /**
     * * WASD movement
     * * Space attack
     * * c pull
     * * v carry
     * * b item
     */
    WASD,
    /**
     * * arrow key movement
     * * j attack
     * * k pull
     * * l carry
     * * i item
     */
    Arrow
}

/**
 * These extension methods of `LocalKeyboardLayout` contain the mapping from more "abstract" actions (attack/pull/...)
 * to concrete keys for the different layouts.
 */
static class LocalControlModeMethods
{
    public static string HorizontalAxis(this LocalKeymap mode)
    {
        switch (mode)
        {
            case LocalKeymap.WASD:
                return "Horizontal";
            case LocalKeymap.Arrow:
                return "HorizontalArrows";
        }

        return "Horizontal";
    }

    public static string VerticalAxis(this LocalKeymap mode)
    {
        switch (mode)
        {
            case LocalKeymap.WASD:
                return "Vertical";
            case LocalKeymap.Arrow:
                return "VerticalArrows";
        }

        return "Vertical";
    }
    
    public static KeyCode AttackKey(this LocalKeymap mode)
    {
        switch (mode)
        {
            case LocalKeymap.WASD:
                return KeyCode.Space;
            case LocalKeymap.Arrow:
                return KeyCode.J;
        }

        return KeyCode.Space;
    }
    
    public static KeyCode PullKey(this LocalKeymap mode)
    {
        switch (mode)
        {
            case LocalKeymap.WASD:
                return KeyCode.C;
            case LocalKeymap.Arrow:
                return KeyCode.K;
        }

        return KeyCode.C;
    }

    public static KeyCode PauseKey(this LocalKeymap keymap)
    {
        switch (keymap)
        {
            case LocalKeymap.WASD:
                return KeyCode.R;
            case LocalKeymap.Arrow:
                return KeyCode.P;
        }

        return KeyCode.P;
    }

    public static KeyCode CarryKey(this LocalKeymap mode)
    {
        switch (mode)
        {
            case LocalKeymap.WASD:
                return KeyCode.V;
            case LocalKeymap.Arrow:
                return KeyCode.L;
        }

        return KeyCode.V;
    }

    public static KeyCode BombKey(this LocalKeymap mode)
    {
        switch (mode)
        {
            case LocalKeymap.WASD:
                return KeyCode.B;
            case LocalKeymap.Arrow:
                return KeyCode.I;
        }

        return KeyCode.B;
    }

    public static KeyCode ThrowKey(this LocalKeymap mode)
    {
        switch (mode)
        {
            case LocalKeymap.WASD:
                return KeyCode.T;
            case LocalKeymap.Arrow:
                return KeyCode.Z;
        }

        return KeyCode.T;
    }
}
