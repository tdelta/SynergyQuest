using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/**
 * <summary>
 * Controls the behavior of the "Credits" menu screen.
 * 
 * Also reads information about external assets (authors, licenses, ...) from the "Resources/ExternalArtCredits.yaml"
 * resource and displays each entry using <see cref="CreditsBlock"/>.
 * </summary>
 */
public class Credits : MonoBehaviour
{
    [SerializeField] private GameObject autoGeneratedCreditsContainer = default;
    [SerializeField] private CreditsBlock creditsBlockPrefab = default;

    private void Awake()
    {
        // Read asset credits information from YAML resource
        var externalArtCreditsRaw = Resources.Load<TextAsset>("ExternalArtCredits").text;
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var externalArtCredits = deserializer.Deserialize<
            Dictionary<string, CreditsEntry>
        >(externalArtCreditsRaw);

        // Display each entry
        foreach (var keyValue in externalArtCredits)
        {
            var block = Instantiate(creditsBlockPrefab, autoGeneratedCreditsContainer.transform);
            block.Init(keyValue.Key, keyValue.Value);
        }
    }

    public void OnViewLicensePressed()
    {
        // FIXME
        Debug.LogError("TO BE IMPLEMENTED");
    }

    public void OnBackPressed()
    {
        SceneController.Instance.LoadMainMenu();
    }
}

