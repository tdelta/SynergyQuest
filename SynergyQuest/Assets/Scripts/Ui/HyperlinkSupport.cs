using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * <summary>
 * Opens hyperlinks in TextMeshPro components on click.
 * </summary>
 * <remarks>
 * Based on https://deltadreamgames.com/unity-tmp-hyperlinks/
 * </remarks>
 */
[RequireComponent(typeof(TextMeshProUGUI))]
public class HyperlinkSupport : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, UnityEngine.Input.mousePosition, null);
        
        // if link was clicked...
        if(linkIndex != -1) {
            var linkInfo = _text.textInfo.linkInfo[linkIndex];
            
            // open the link
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
