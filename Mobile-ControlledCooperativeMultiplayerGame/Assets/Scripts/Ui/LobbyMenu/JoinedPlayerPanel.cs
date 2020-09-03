using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * <summary>
 * Renders a player sprite with appropriate color and name for a given <see cref="ControllerInput"/> in the UI.
 * It is used by <see cref="LobbyMenuUi"/> to display a list of connected players.
 *
 * You must call <see cref="Init"/> after <see cref="Object.Instantiate"/>
 * </summary>
 */
public class JoinedPlayerPanel : MonoBehaviour
{
    [SerializeField] private Image playerSpriteImage = default;
    [SerializeField] private TextMeshProUGUI playerNameLabel = default;
    
    private static readonly int ShirtColorProperty = Shader.PropertyToID("_ShirtColor");

    public void Init(ControllerInput playerInput)
    {
        var modMaterial = Instantiate(playerSpriteImage.material);
        modMaterial.SetColor(ShirtColorProperty, playerInput.GetColor().ToRGB());
        playerSpriteImage.material = modMaterial;
        
        playerNameLabel.SetText(playerInput.PlayerName);
    }
}
