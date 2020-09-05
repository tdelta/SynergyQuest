using System.IO;
using TMPro;
using UnityEngine;
using WebSocketSharp;

/**
 * <summary>
 * Displays an instance of <see cref="CreditsEntry"/> in the UI.
 * </summary>
 */
public class CreditsBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI authorValue;
    [SerializeField] private TextMeshProUGUI fileValue;
    [SerializeField] private TextMeshProUGUI licenseValue;
    [SerializeField] private TextMeshProUGUI descriptionValue;
    [SerializeField] private TextMeshProUGUI modificationsValue;
    [SerializeField] private TextMeshProUGUI linkValue;

    public void Init(string filePath, CreditsEntry entry)
    {
        authorValue.SetText(entry.author);
        fileValue.SetText(Path.GetFileName(filePath));
        licenseValue.SetText(entry.license);
        descriptionValue.SetText(entry.description);
        modificationsValue.SetText(entry.modifications.IsNullOrEmpty() ? "None" : entry.modifications);
        linkValue.SetText(entry.link);
    }
}
