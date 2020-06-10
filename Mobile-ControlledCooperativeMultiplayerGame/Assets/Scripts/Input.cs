
/**
 * Models the different inputs for a player character.
 *
 * The interface allows to abstract from the fact whether an input is local (keyboard) or remote (smartphone controller).
 * For a local implementation, see class `LocalInput`.
 * For a remote implementation see class `ControllerInput`.
 */
public interface Input
{
    /**
     * Returns whether a specific button is currently pressed or not.
     * For the different button ids, see the `Button` enum.
     */
    bool GetButton(Button button);
    
    /**
     * Returns whether a specific button has been pressed during the current frame.
     * It will not return true during the next frames until the button has been pressed again.
     * 
     * For the different button ids, see the `Button` enum.
     */
    bool GetButtonDown(Button button);
    
    /**
     * Returns whether a specific button has been released during the current frame.
     * It will not return true during the next frames until the button has been pressed and released again.
     * 
     * For the different button ids, see the `Button` enum.
     */
    bool GetButtonUp(Button button);

    
    /**
     * Returns the value of the Joystick position on the vertical axis.
     * 
     * The value is in [-1; 1], at least implementors are required to only return
     * such values.
     */
    float GetVertical();
    
    /**
     * Returns the value of the Joystick position on the horizontal axis.
     * 
     * The value is in [-1; 1], at least implementors are required to only return
     * such values.
     */
    float GetHorizontal();
}
