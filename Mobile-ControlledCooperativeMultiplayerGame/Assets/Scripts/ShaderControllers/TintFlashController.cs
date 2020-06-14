using UnityEngine;

/**
 * This script allows to briefly color a sprite with a certain tint.
 * For example to tint an enemy red when hit.
 *
 * It must be used together with the `TintMaterial` on the sprite.
 */
public class TintFlashController : MonoBehaviour
{
    /**
     * The controller can be in three states:
     *
     * 1. No tint shall currently be applied
     * 2. A tint is applied with increasing intensity
     * 3. A tint is removed by gradually lowering its intensity
     */
    enum TintFlashState
    {
        NoTint,
        IncreasingTint,
        RemovingTint
    }
    
    /**
     * The material which is controlled.
     */
    private Material _material;
    
    /**
     * The color of the tint which shall be applied.
     * It is set by the `FlashTint` method.
     */
    private Color _tintColor;
    
    /**
     * The speed with which the intensity of the tint currently changes.
     * It is set by the `FlashTint` method and linearly interpolates the the intensity of the tint for the target
     * duration.
     */
    private float _tintSpeed; // color units / second
    
    private TintFlashState _tintState;
    
    private static readonly int TintProperty = Shader.PropertyToID("_Tint");
    
    /**
     * Applies a tint for a fixed duration. The intensity of the tint will rise to maximum until the half time of the
     * duration. Afterwards it decreases to zero.
     */
    public void FlashTint(Color tintColor, float duration)
    {
        _tintColor = tintColor;
        _tintColor.a = 0; // The alpha value controls the intensity of the tint in this shader. Start with no tint.
        
        // Compute a rate of change of tint so that maximum tint is reached halfway of the duration.
        _tintSpeed = 1 / (duration / 2);
        
        _tintState = TintFlashState.IncreasingTint;
    }

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // If we are currently applying a tint...
        if (_tintState != TintFlashState.NoTint)
        {
            // Modify it by the current rate of change and apply it
            _tintColor.a = Mathf.Clamp01(_tintColor.a + _tintSpeed * Time.deltaTime);
            _material.SetColor(TintProperty, _tintColor);
        }
        
        switch (_tintState)
        {
            case TintFlashState.IncreasingTint:
                // If we are currently increasing the tint, stop when it reaches 1
                if (_tintColor.a >= 1)
                {
                    // When it has reached 1, start decreasing the tint again
                    _tintState = TintFlashState.RemovingTint;
                    _tintSpeed = -_tintSpeed;
                }
                break;
            
            case TintFlashState.RemovingTint:
                // If we are currently decreasing the tint, stop when it reaches 0
                if (_tintColor.a <= 0)
                {
                    _tintState = TintFlashState.NoTint;
                    _tintSpeed = 0;
                }
                break;
        }
    }
}
