using UnityEngine;

/**
 * Sign which displays an info screen if touched by a player.
 */
public class Sign : MonoBehaviour
{
    // Info screen content to display
    [SerializeField] private InfoScreenContent content;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            InfoScreenLauncher.Instance.Launch(content);
        }
    }
}
