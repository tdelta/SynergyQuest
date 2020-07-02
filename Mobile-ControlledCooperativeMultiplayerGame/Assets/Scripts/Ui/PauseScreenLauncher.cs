/**
 * Singleton which allows to launch and control the pause screen UI and pause the game.
 * It also changes the state of remote controllers appropriately
 * (by enabling / disabling menu actions etc.)
 *
 * For the actual pause screen UI behavior see the `PauseScreenUi` class.
 * The UI prefab used by this singleton can be set in the `MenuPrefabSettings` instance in the `Resources` folder.
 *
 * !!! See also the base class `MenuLauncher` for a description of the inherited methods.
 */
public class PauseScreenLauncher: MenuLauncher<PauseScreenLauncher, PauseScreenUi>
{
    private bool _isPaused = false;
    public bool IsPaused => _isPaused;
    
    /**
     * Opens the pause screen and pauses the game.
     */
    public void Launch()
    {
        // We can directly call the base class method without a callback, since the pause screen UI behavior does not
        // need any special initialization steps
        Launch(null);
    }
    
    protected override PauseScreenUi GetUiPrefab()
    {
        return MenuPrefabSettings.Instance.pauseScreenUiPrefab;
    }

    protected override void OnUiLaunched()
    {
        _isPaused = true;
    }

    protected override void OnUiClosed()
    {
        _isPaused = false;
    }
}
