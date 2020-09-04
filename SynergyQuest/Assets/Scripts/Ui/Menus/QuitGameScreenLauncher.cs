/**
 * Singleton which allows to launch and control the UI screen which allows players to close the game.
 * It also changes the state of remote controllers appropriately
 * (by enabling / disabling menu actions etc.)
 *
 * For the actual UI behavior see the `QuitGameScreenUi` class.
 * The UI prefab used by this singleton can be set in the `MenuPrefabSettings` instance in the `Resources` folder.
 *
 * !!! See also the base class `MenuLauncher` for a description of the inherited methods.
 */
public class QuitGameScreenLauncher: MenuLauncher<QuitGameScreenLauncher, QuitGameScreenUi>
{
    /**
     * Opens the screen and pauses the game.
     */
    public void Launch()
    {
        // We can directly call the base class method without a callback, since the UI behavior of this screen does not
        // need any special initialization steps
        Launch(null);
    }
    
    protected override QuitGameScreenUi GetUiPrefab()
    {
        return MenuPrefabSettings.Instance.quitGameScreenUi;
    }
}
