
/**
 * Singleton which allows to launch and control the info screen UI.
 * It also pauses the game and changes the state of remote controllers appropriately
 * (by enabling / disabling menu actions etc.)
 *
 * An info screen is a set of pages with information displayed to players, for example when reading a sign.
 *
 * For the actual info screen UI behavior see the `InfoScreenUi` class.
 * The UI prefab used by this singleton can be set in the `MenuPrefabSettings` instance in the `Resources` folder.
 *
 * !!! See also the base class `MenuLauncher` for a description of the inherited methods.
 */
public class InfoScreenLauncher: MenuLauncher<InfoScreenLauncher, InfoScreenUi>
{
    private bool _isShowingInfoScreen = false;
    public bool IsShowingInfoScreen => _isShowingInfoScreen;

    /**
     * Opens an info screen and pauses the game.
     *
     * @param content the content that shall be displayed by the info screen.
     */
    public void Launch(InfoScreenContent content)
    {
        Launch(ui => ui.Init(content));
    }

    /**
     * Display the next page of the info screen, if there is one and if the screen is active.
     */
    public void NextPage()
    {
        if (UiInstance != null)
        {
            UiInstance.OnNextButton();
        }
    }

    /**
     * Display the previous page of the info screen, if there is one and if the screen is active.
     */
    public void PreviousPage()
    {
        if (UiInstance != null)
        {
            UiInstance.OnPrevButton();
        }
    }
    
    protected override InfoScreenUi GetUiPrefab()
    {
        return MenuPrefabSettings.Instance.infoScreenUiPrefab;
    }
    
    protected override void OnUiLaunched()
    {
        _isShowingInfoScreen = true;
    }

    protected override void OnUiClosed()
    {
        _isShowingInfoScreen = false;
    }
}
