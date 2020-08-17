using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class ColorReplacer : MonoBehaviour
{
    [SerializeField] private Color colorToReplace = Color.green;
    [FormerlySerializedAs("_replacementColor")] [SerializeField] private Color replacementColor = Color.green;
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
