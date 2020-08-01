using UnityEngine;

/**
 * Sign which displays an info screen if touched by a player.
 */
public class Sign : MonoBehaviour
{
    // Info screen content to display
    [SerializeField] private InfoScreenContent content;

    public void Show()
    {
        InfoScreenLauncher.Instance.Launch(content);
    }
}
