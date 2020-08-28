using System;
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

    private SpriteRenderer _renderer;

    private MaterialPropertyBlock _materialProperties = null;
    private static readonly int ReplacementColorProperty = Shader.PropertyToID("_ReplacementColor");
    private static readonly int ColorToReplaceProperty = Shader.PropertyToID("_ColorToReplace");
    
    void Awake()
    {
        PerformReplacement();
    }

    private void OnValidate()
    {
        PerformReplacement();
    }

    private void PerformReplacement()
    {
        EnsureReady();
        
        _renderer.GetPropertyBlock(_materialProperties);
        
        _materialProperties.SetColor(ColorToReplaceProperty, colorToReplace);
        _materialProperties.SetColor(ReplacementColorProperty, replacementColor);
        
        _renderer.SetPropertyBlock(_materialProperties);
    }

    private void EnsureReady()
    {
        if (ReferenceEquals(_materialProperties, null))
        {
            _materialProperties = new MaterialPropertyBlock();
        }

        if (ReferenceEquals(_renderer, null))
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
    }
}
