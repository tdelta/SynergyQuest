using UnityEngine;

/**
 * A singleton which listens to `MenuAction`s sent by remote controllers for the purpose of opening menus
 */
public class GlobalMenuActionListener: BehaviourSingleton<GlobalMenuActionListener>
{
    /**
     * Execute `OnAwake` in Unity to ensure this singleton is instantiated.
     */
    [RuntimeInitializeOnLoadMethod]
    public static void EnsureInitialization()
    {
        var _ = Instance;
    }
    
    private void OnEnable()
    {
        SharedControllerState.Instance.OnMenuActionTriggered += OnMenuActionTriggered;
    }

    private void OnDisable()
    {
        SharedControllerState.Instance.OnMenuActionTriggered -= OnMenuActionTriggered;
    }

    private void OnMenuActionTriggered(MenuAction action)
    {
        switch (action)
        {
            case MenuAction.PauseGame:
                PauseScreenLauncher.Instance.Launch();
                break;
            case MenuAction.ShowMap:
                InfoScreenLauncher.Instance.Launch(MapPrefabSettings.Instance.MapInfoScreenContent);
                break;
        }
    }
}
