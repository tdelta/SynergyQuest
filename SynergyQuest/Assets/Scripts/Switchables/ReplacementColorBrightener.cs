using UnityEngine;

/**
 * <summary>
 * The <see cref="ColorReplacer"/> component allows to replace colors in sprites. This component allows to temporarily
 * brighten this color.
 * This is for example useful to show that a colored switch activates, e.g. <see cref="ColorSwitch"/>.
 * </summary>
 */
[RequireComponent(typeof(Switchable), typeof(ColorReplacer))]
public class ReplacementColorBrightener : MonoBehaviour
{
    /**
     * How much shall this component temporarily brighten the replacement color?
     */
    [SerializeField] private float brightnessDelta = 0.2f;
    
    private Switchable _switchable;
    private ColorReplacer _colorReplacer;

    // We cache the colors in these fields
    private Color _beforeActivationColor; // Color used by the ColorReplacer before applying this effect

    private void Awake()
    {
        _colorReplacer = GetComponent<ColorReplacer>();
        _switchable = GetComponent<Switchable>();
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Start()
    {
        _beforeActivationColor = _colorReplacer.ReplacementColor;
        
        // The Switchable component does not trigger this callback by itself for the initial activation when loading the
        // scene. Hence, we look the initial value up ourselves
        OnActivationChanged(_switchable.Activation);
    }

    void OnActivationChanged(bool activation)
    {
        if (activation)
        {
            _beforeActivationColor = _colorReplacer.ReplacementColor;
            
            _colorReplacer.ReplacementColor = new Color(
                Mathf.Min(1.0f, _beforeActivationColor.r + brightnessDelta),
                Mathf.Min(1.0f, _beforeActivationColor.g + brightnessDelta),
                Mathf.Min(1.0f, _beforeActivationColor.b + brightnessDelta),
                _beforeActivationColor.a
            );
        }

        else
        {
            _colorReplacer.ReplacementColor = _beforeActivationColor;
        }
    }
}
