using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/**
 * <summary>
 * Controls the behavior of the "License" menu screen.
 * </summary>
 */
public class License : MonoBehaviour
{
    public void OnBackPressed()
    {
        SceneController.Instance.LoadCredits();
    }
}

