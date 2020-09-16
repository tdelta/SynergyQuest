using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    [SerializeField] private TextMeshProUGUI authorValue = default;
    [SerializeField] private TextMeshProUGUI fileValue = default;
    [SerializeField] private TextMeshProUGUI licenseValue = default;
    [SerializeField] private TextMeshProUGUI descriptionValue = default;
    [SerializeField] private TextMeshProUGUI modificationsValue = default;
    [SerializeField] private TextMeshProUGUI linkValue = default;

    public void Init(string filePath, CreditsEntry entry)
    {
        authorValue.SetText(entry.author);
        fileValue.SetText(Path.GetFileName(filePath));
        licenseValue.SetText(entry.license);
        descriptionValue.SetText(entry.description);
        modificationsValue.SetText(entry.modifications.IsNullOrEmpty() ? "None" : entry.modifications);
        
        linkValue.SetText(
            string.Join(
                " ",
                entry.link
                    .Split(' ')
                    .Select(token =>
                        token.StartsWith("http") ?
                            $"<u><link={token}>{token}</link></u>"
                            : token
                    )
                    .ToArray()
            )
        );
    }
}
