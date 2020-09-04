/**
 * <summary>
 * Hint to controller, what inputs are currently expected from this specific controller by the game.
 *
 * For example, if the mode is set to <c>IMUOrientation</c>, the controller should adapt its display to give the user
 * visual feedback about the 3d orientation of the device. On the other hand, it does not need to display the joystick
 * and most buttons, since joystick input will be ignored by the game.
 *
 * This is different from <see cref="GameState"/> which is global to all controllers and also indicates the overall
 * state of the game.
 * </summary>
 */
public enum InputMode
{
    // input from all buttons, menu actions, joystick etc.
    Normal = 0,
    // orientation input from IMU sensors is expected. Controller does not need to display joystick or buttons (except the `Exit` button). Menus should still be displayed
    IMUOrientation = 1,
}
