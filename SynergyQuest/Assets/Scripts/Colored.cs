using UnityEngine;

/**
 * <summary>
 * Players have an identifying color, <see cref="PlayerColor"/> which dictates whether or not they can interact with
 * certain objects or not.
 * 
 * This component assigns an identifying color to an object. If the color is changed in the editor, other
 * color-controlled components like <see cref="ColorSwitch"/> or <see cref="ColorReplacer"/> are programmed to use the
 * same color.
 *
 * Furthermore, if this object has a <see cref="Renderer"/> which uses an material which supports
 * <see cref="PlayerColorShaderProperty"/>, the property is set to the assigned color.
 * </summary>
 */
public class Colored : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;

    public PlayerColor Color
    {
        get => color;
        set
        {
            if (color != value)
            {
                color = value;
                SetupShader();
                OnColorChanged?.Invoke(value);
            }
        }
    }

    /**
     * <summary>
     * Event which is invoked whenever the value of <see cref="Color"/> is changed
     * </summary>
     */
    public event ColorChangedAction OnColorChanged;
    public delegate void ColorChangedAction(PlayerColor newColor);

    private void Awake()
    {
        SetupShader();
    }

    private void OnValidate()
    {
        SetupShader();
        // Other components like `ColorReplacer` may depend on the color value of this component.
        // Hence we run the OnValidate methods of all other components, when this component is changed
        this.ValidateOtherComponents();
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
