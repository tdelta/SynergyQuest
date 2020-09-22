using System.IO;
using TMPro;
using UnityEngine;

/**
 * <summary>
 * Controls the behavior of the "License" menu screen.
 * </summary>
 */
public class License : MonoBehaviour
{
    [Tooltip("Path to file containing the game license when the game is executed in Debug mode (Unity Editor)")]
    [SerializeField] private string pathToLicenseDebugMode = "../LICENSE.md";
    [Tooltip("Path to file containing the game license when the game is executed in production mode (standalone executable)")]
    [SerializeField] private string pathToLicenseProductionMode = "LICENSE.md";

    [SerializeField] private TextMeshProUGUI licenseTextField;

    private void Awake()
    {
        licenseTextField.SetText(LoadLicenseText());
    }

    private string LoadLicenseText()
    {
        var path = Path.Combine(
            PathUtils.GetInstallDirectory().CorrectFsSlashes(),
            (DebugSettings.Instance.DebugMode
                    ? pathToLicenseDebugMode
                    : pathToLicenseProductionMode
                ).CorrectFsSlashes()
        );

        return File.ReadAllText(path);
    }

    public void OnBackPressed()
    {
        SceneController.Instance.LoadCredits();
    }
}

