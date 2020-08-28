using UnityEngine;

/**
 * <summary>
 * Players have an identifying color, <see cref="PlayerColor"/> which dictates whether or not they can interact with
 * certain objects or not.
 * 
 * This component assigns an identifying color to an object. If the color is changed in the editor, it makes sure other
 * color-controlled components like <see cref="ColorSwitch"/> or <see cref="ColorReplacer"/> use the same color.
 *
 * Furthermore, if this object has a <see cref="Renderer"/> which uses an material which supports
 * <see cref="PlayerColorShaderProperty"/>, the property is set to the assigned color.
 * </summary>
 */
public class Colored : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    public PlayerColor Color => color;

    private void Awake()
    {
        SetupShader();
    }

    private void OnValidate()
    {
        if (GetComponent<ColorSwitch>() is ColorSwitch colorSwitch)
        {
            colorSwitch.Color = color;
        }

        if (GetComponent<ColorReplacer>() is ColorReplacer colorReplacer)
        {
            colorReplacer.ReplacementColor = color.ToRGB();
        }
        
        SetupShader();
    }

    private static readonly int PlayerColorShaderProperty = Shader.PropertyToID("_PlayerColor");

    private void SetupShader()
    {
        // If this object has a renderer, and it supports the player color property, then assign the color value of this
        // component to it.
        if (
            TryGetComponent(out Renderer renderer) &&
            renderer.sharedMaterial.HasProperty(PlayerColorShaderProperty)
        )
        {
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
                
            properties.SetColor(PlayerColorShaderProperty, color.ToRGB());
                
            renderer.SetPropertyBlock(properties);
        }
    }
}
