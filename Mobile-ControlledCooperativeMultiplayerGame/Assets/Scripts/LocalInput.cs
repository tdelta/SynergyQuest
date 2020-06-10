
using UnityEngine;

/**
 * Implements input for a player character from the local keyboard.
 * See `Input` interface.
 */
public class LocalInput: Singleton<LocalInput>, Input
{
    public bool GetButton(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKey(KeyCode.Space);
            case Button.Pull:
                return UnityEngine.Input.GetKey(KeyCode.P);
        }

        return false;
    }

    public bool GetButtonDown(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKeyDown(KeyCode.Space);
            case Button.Pull:
                return UnityEngine.Input.GetKeyDown(KeyCode.P);
        }

        return false;
    }
    
    public bool GetButtonUp(Button button)
    {
        switch (button)
        {
            case Button.Attack:
                return UnityEngine.Input.GetKeyUp(KeyCode.Space);
            case Button.Pull:
                return UnityEngine.Input.GetKeyUp(KeyCode.P);
        }

        return false;
    }

    public float GetVertical()
    {
        return UnityEngine.Input.GetAxis("Vertical");
    }

    public float GetHorizontal()
    {
        return UnityEngine.Input.GetAxis("Horizontal");
    }
}
