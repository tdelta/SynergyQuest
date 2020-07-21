using TMPro;
using UnityEngine;

/**
 * Controls the UI of info screens.
 * To actually launch an info screen, see the `InfoScreenLauncher` singleton.
 *
 * See also the `MenuUi` interface for a description of its methods.
 */
public class InfoScreenUi: MonoBehaviour, MenuUi
{
    /**
     * UI element displaying the title of the info screen
     */
    [SerializeField] private TextMeshProUGUI header;
    /**
     * UI element displaying the subtitle of the info screen
     */
    [SerializeField] private TextMeshProUGUI subHeader;
    /**
     * Button which goes back to the previous page
     */
    [SerializeField] private GameObject backButton;
    /**
     * Text of the button which selects the next page or closes the info screen.
     * The text is changed depending on whether there is a next page.
     */
    [SerializeField] private TextMeshProUGUI nextButtonText;
    /**
     * UI element which contains the text of the info screen.
     * Its first child is expected to be an element containing the titles.
     * The second child is modified by this class to contain the actual text of the current page.
     */
    [SerializeField] private GameObject pageContent;

    /**
     * Content displayed in this UI. To be set by the `Init` method.
     */
    private InfoScreenContent _content;
    private int _currentPage = 0;

    /**
     * After instantiating the UI, this method must be called to dynamically supply the info to be displayed.
     */
    public void Init(InfoScreenContent content)
    {
        _content = content;
        
        header.SetText(content.headerText);
        subHeader.SetText(content.subHeaderText);
    }
    
    /**
     * Displays the next page of the information, if there is one.
     * Otherwise, it asks `InfoScreenLauncher` to close the info screen.
     */
    public void OnNextButton()
    {
        if (HasNextPage())
        {
            ++_currentPage;
            DrawContent();
        }

        else
        {
            InfoScreenLauncher.Instance.Close();
        }
    }

    /**
     * Displays the previous page of the information, if there is one.
     */
    public void OnPrevButton()
    {
        if (HasPreviousPage())
        {
            --_currentPage;
            DrawContent();
        }
    }

    /**
     * Dynamically updates the UI to display the contents of the current page.
     * It also adapts the menu buttons (next, back, resume) displayed on remote controllers.
     */
    void DrawContent()
    {
        // Determine, which actions / buttons shall be available
        var nextActionAvailable = HasNextPage();
        var resumeActionAvailable = !nextActionAvailable;
        var backActionAvailable = HasPreviousPage();
        
        // Enabling / disabling remote controller buttons
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, resumeActionAvailable),
            (MenuAction.Next, nextActionAvailable),
            (MenuAction.Back, backActionAvailable)
        );
        
        // Modifying local buttons
        // The back button shall only be active, if there is a previous page
        backButton.SetActive(backActionAvailable);
        
        string nextButtonTextString = "Next";
        // If there is no next page, change the text of the "Next" button to "Resume"
        if (!nextActionAvailable)
        {
            nextButtonTextString = "Resume";
        }
        nextButtonText.SetText(nextButtonTextString);

        // If there is currently old information text displayed, remove it first.
        if (pageContent.transform.childCount > 1)
        {
            var oldPage = pageContent.transform.GetChild(1);
            Destroy(oldPage.gameObject);
        }

        // Now we can add the text of the next page instead.
        Instantiate(_content.pagePrefabs[_currentPage], pageContent.transform);
    }

    
    public void OnLaunch()
    {
        // Dynamically draw the content as soon as the menu base UI has been loaded
        DrawContent();
    }

    public void OnClose()
    {
        // Disable relevant menu actions (next page, previous page, ...) as they are no longer needed when this U
        // is closed.
        SharedControllerState.Instance.EnableMenuActions(
            (MenuAction.ResumeGame, false),
            (MenuAction.Next, false),
            (MenuAction.Back, false)
        );
    }

    private bool HasNextPage()
    {
        return _currentPage + 1 < _content.pagePrefabs.Length;
    }
    
    private bool HasPreviousPage()
    {
        return _currentPage > 0;
    }
}
