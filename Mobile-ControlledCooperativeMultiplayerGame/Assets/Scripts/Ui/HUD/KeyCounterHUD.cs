using TMPro;
using UnityEngine;

/**
 * Displays the current number of keys the players have collected.
 */
public class KeyCounterHUD : MonoBehaviour
{
    /**
     * Canvas group which contains all elements of this HUD. It is used to make it invisible, iff the number of keys is
     * 0.
     */
    [SerializeField] private CanvasGroup panel;
    /**
     * Text which will be set to the current number of collected keys
     */
    [SerializeField] private TextMeshProUGUI counterText;

    private void OnEnable()
    {
        PlayerDataKeeper.Instance.OnNumKeysChanged += OnNumKeysChanged;
    }

    private void OnDisable()
    {
        PlayerDataKeeper.Instance.OnNumKeysChanged -= OnNumKeysChanged;
    }

    private void Start()
    {
        OnNumKeysChanged(PlayerDataKeeper.Instance.NumKeys);
    }

    /**
     * Invoked, if the number of keys which the players currently have changes.
     */
    private void OnNumKeysChanged(int numKeys)
    {
        // If the players have no keys, make the HUD invisible
        if (numKeys <= 0)
        {
            panel.alpha = 0;
        }

        else
        {
            // Otherwise, make it visible and display the number of keys
            panel.alpha = 1;
            counterText.SetText($"{numKeys}x");
        }
    }
}
