using UnityEngine;

[RequireComponent(typeof(Switchable), typeof(ColorReplacer))]
public class ReplacementColorBrightener : MonoBehaviour
{
    [SerializeField] private float brightnessDelta = 0.2f;
    
    private Switchable _switchable;
    private ColorReplacer _colorReplacer;

    private Color _originalColor;
    private Color _brightenedColor;

    private void Awake()
    {
        _colorReplacer = GetComponent<ColorReplacer>();
        _switchable = GetComponent<Switchable>();

        _originalColor = _colorReplacer.ReplacementColor;
        _brightenedColor = new Color(
            Mathf.Min(1.0f, _originalColor.r + brightnessDelta),
            Mathf.Min(1.0f, _originalColor.g + brightnessDelta),
            Mathf.Min(1.0f, _originalColor.b + brightnessDelta),
            _originalColor.a
        );
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    void OnActivationChanged(bool activation)
    {
        _colorReplacer.ReplacementColor = activation ? _brightenedColor : _originalColor;
    }
}
