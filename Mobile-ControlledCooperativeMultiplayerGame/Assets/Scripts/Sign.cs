using UnityEngine;

/**
 * Sign which displays an info screen if touched by a player.
 */
public class Sign : MonoBehaviour
{
    // Info screen content to display
    [SerializeField] private InfoScreenContent content;

    private Interactive _interactive;
    
    void Start()
    {
        _interactive = GetComponent<Interactive>();
    }

    public void Show()
    {
        InfoScreenLauncher.Instance.Launch(content);
    }
}
