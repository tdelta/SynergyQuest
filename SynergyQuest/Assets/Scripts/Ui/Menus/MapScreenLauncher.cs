/**
 * <summary>
 * Singleton which allows to launch and control the map screen UI.
 * The map screen gives an overview over the dungeon layout.
 * </summary>
 * <remarks>
 * It also pauses the game and changes the state of remote controllers appropriately
 * (by enabling / disabling menu actions etc.)
 *
 * For the actual map screen UI behavior see the <see cref="MapScreenUi"/> class.
 * The UI prefab used by this singleton can be set in the <see cref="MenuPrefabSettings"/> instance in the
 * <c>Resources</c> folder.
 *
 * !!! See also the base class <see cref="MenuLauncher{LauncherType,UiType}"/> for a description of the inherited methods.
 * </remarks>
 */
public class MapScreenLauncher: MenuLauncher<MapScreenLauncher, MapScreenUi>
{
    public void Launch()
    {
        Launch(null);
    }
    
    protected override MapScreenUi GetUiPrefab()
    {
        return MenuPrefabSettings.Instance.MapScreenUiPrefab;
    }
}
