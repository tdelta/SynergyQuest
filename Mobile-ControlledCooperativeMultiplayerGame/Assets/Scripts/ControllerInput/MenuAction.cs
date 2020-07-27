/**
 * Identifiers of the different "menu actions" which can be enabled/disabled for controllers
 */
public enum MenuAction
{
    StartGame = 0,
    QuitGame = 1,
    PauseGame = 2,
    ResumeGame = 3,
    Next = 4, // Info screens can have multiple pages, which can be browsed with this menu action
    Back = 5,
    Yes = 6,
    No = 7,
}
