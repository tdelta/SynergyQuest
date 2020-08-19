using UnityEngine;
using UnityEngine.Serialization;

/**
 * <summary>
 * Can swap out a specific color from a sprite. E.g. parts of the sprite of player controlled platforms,
 * <see cref="PlayerControlledPlatform"/>, are colored with the color assigned to the player controlling them.
 *
 * This requires the <see cref="SpriteRenderer"/> of the object using this component to use a material which supports
 * the following properties:
 * <list type="bullet">
 *   <item><description><c>_ReplacementColor</c></description></item>
 *   <item><description><c>_ColorToReplace</c></description></item>
 * </list>
 *
 * E.g. a material using the <c>ColorReplacer</c> shader.
 * </summary>
 */
[RequireComponent(typeof(SpriteRenderer))]
public class ColorReplacer : MonoBehaviour
{
    /**
     * Color which shall be replaced in sprites.
     */
    [SerializeField] private Color colorToReplace = Color.green;
    /**
     * The color with which the color shall be replaced.
     */
    [FormerlySerializedAs("_replacementColor")]
    [SerializeField]
    private Color replacementColor = Color.green;
    public Color ReplacementColor
    {
        get
        {
            return replacementColor;
        }
        set
        {
            replacementColor = value;
            PerformReplacement();
        }
    }

    /**
     * The material which is controlled.
     */
    private Material _material;
    private static readonly int ReplacementColorProperty = Shader.PropertyToID("_ReplacementColor");
    private static readonly int ColorToReplaceProperty = Shader.PropertyToID("_ColorToReplace");
    
    void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
        PerformReplacement();
    }

    private void PerformReplacement()
    {
        // ReSharper disable once Unity.NoNullPropagation
        if (_material != null)
        {
            _material.SetColor(ColorToReplaceProperty, colorToReplace);
            _material.SetColor(ReplacementColorProperty, replacementColor);
        }
    }
}
