using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * <summary>
 * Allows to display a speech bubble with a custom text.
 * </summary>
 * <remarks>
 * Don't directly assign to an object. Instead use the static method <see cref="Display"/> to create and display a
 * speech bubble.
 * This class uses the prefab set in <see cref="PrefabSettings"/>.
 * </remarks>
 */
public class SpeechBubble : MonoBehaviour
{
    [Tooltip("The text of the speech bubble will be displayed using this object.")]
    [SerializeField] private TextMeshProUGUI text = default;
    [Tooltip("All parent layout groups wrapping the text sorted from inside to outside. Will be rebuilt when text changes.")]
    [SerializeField] private LayoutGroup[] layoutGroups = default;

    /**
     * <summary>
     * Instantiates and displays a speech bubble.
     * </summary>
     * <param name="position">Where the speech bubble will be displayed (lower left corner)</param>
     * <param name="text">Text to display. The rich text features of TextMeshPro (tags) are supported.</param>
     * <param name="duration">How long the speech bubble will be displayed. It will not be destroyed automatically if set to 0 (default)</param>
     * <returns>the created <see cref="SpeechBubble"/> instance</returns>
     */
    public static SpeechBubble Display(Vector3 position, string text, float duration = 0)
    {
        var instance = Instantiate(PrefabSettings.Instance.SpeechBubblePrefab, position, Quaternion.identity);
        
        instance.text.SetText(text);
        // When changing the text, the layout groups need to be rebuild from the innermost to the outermost to adapt to
        // the changed text size
        foreach (var layoutGroup in instance.layoutGroups.SelectNotNull(layoutGroup => layoutGroup.GetComponent<RectTransform>()))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
        }

        if (duration > 0)
        {
            Destroy(instance.gameObject, duration);
        }

        return instance;
    }

    private void Start()
    {
        // Make sure the speech bubble is visible on screen by making the cameras follow it
        this.gameObject.SetFollowedByCamera(
            true,
            ((RectTransform) this.transform).rect.size.MaxComponent()
        );
    }

    private void OnDestroy()
    {
        this.gameObject.SetFollowedByCamera(false);
    }
}
