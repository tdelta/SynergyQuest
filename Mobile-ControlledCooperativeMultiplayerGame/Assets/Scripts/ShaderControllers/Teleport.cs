using UnityEngine;

/**
 * <summary>
 * Can display a dissolving / teleporting effect of a sprite with different colors using shaders.
 * E.g. players are teleported away using this shader when starting to control a platform, <see cref="PlayerControlledPlatform"/>.
 *
 * You should not directly assign this component to a game object. Instead call the <see cref="TeleportIn"/> and
 * <see cref="TeleportOut"/> methods with the object to apply the effect to any object with a compatible material /
 * shader.
 *
 * The implementation of the shaders used by this material is based on these videos:
 * <list type="bullet">
 *   <item><description><a href="https://www.youtube.com/watch?v=taMp1g1pBeE">Video by Brackeys</a></description></item>
 *   <item><description><a href="https://www.youtube.com/watch?v=dImQy_K5zuk">Video by The Game Dev Shack</a></description></item>
 * </list>
 *
 * This requires the <see cref="SpriteRenderer"/> of the object using this component to use a material which supports
 * the following properties:
 * <list type="bullet">
 *   <item><description><c>_TeleportColor</c></description></item>
 *   <item><description><c>_TeleportProgress</c></description></item>
 * </list>
 *
 * This can easily be implemented by using the <c>Shaders/SubGraphs/TeleportEffect</c> shader sub-graph in the shader of
 * the material.
 * </summary>
 */
[RequireComponent(typeof(SpriteRenderer))]
public class Teleport : MonoBehaviour
{
    private float _speed = 1.0f;
    private float _timer = 0.0f;
    private bool _isTeleportingOut;

    private Material _material;
    
    private static readonly int ColorProperty = Shader.PropertyToID("_TeleportColor");
    /**
     * This shader property controls the progress of the animation.
     * It ranges from 0 to 1.
     */
    private static readonly int ProgressProperty = Shader.PropertyToID("_TeleportProgress");
    
    void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;

        if (!_material.HasProperty(ColorProperty) || !_material.HasProperty(ProgressProperty))
        {
            Debug.LogError("Trying to apply teleport effect to sprite with a material which does not support teleport effects.");
        }
    }

    /**
     * <summary>
     * Play the teleport effect on an visible object to make it disappear.
     * The objects renderer must fulfill the requirements discussed in the class description.
     *
     * Furthermore, the object will be made unable to move by itself and permanently invisible after the effect finishes
     * playing, <see cref="BehaviourExtensions.Freeze"/> and <see cref="BehaviourExtensions.MakeInvisible"/>.
     * </summary>
     * <param name="go">Object which shall be teleported away</param>
     * <param name="color">Color of the teleport effect.</param>
     * <param name="speed">
     *   Speed modifier for the teleport effect.
     *   E.g. use 2.0f for twice the speed and 0.5f for half the speed.
     * </param>
     */
    public static void TeleportOut(GameObject go, Color color, float speed = 0.75f)
    {
        go.Freeze();
    
        var instance = go.AddComponent<Teleport>();
        instance._speed = speed;
        instance._isTeleportingOut = true;
        instance._material.SetColor(ColorProperty, color);
        instance._material.SetFloat(ProgressProperty, 0.0f);
    }
    
    /**
     * <summary>
     * Play the teleport effect on an invisible object to make it appear.
     * The objects renderer must fulfill the requirements discussed in the class description.
     * 
     * Furthermore, the object will be made able to move by itself (again) and permanently visible,
     * <see cref="BehaviourExtensions.UnFreeze"/> and <see cref="BehaviourExtensions.MakeVisible"/>.
     * </summary>
     * <param name="go">Object which shall be teleported in</param>
     * <param name="color">Color of the teleport effect.</param>
     * <param name="speed">
     *   Speed modifier for the teleport effect.
     *   E.g. use 2.0f for twice the speed and 0.5f for half the speed.
     * </param>
     */
    public static void TeleportIn(GameObject go, Color color, float speed = 0.75f)
    {
        var instance = go.AddComponent<Teleport>();
        instance._speed = speed;
        instance._isTeleportingOut = false;
        instance._material.SetColor(ColorProperty, color);
        instance._material.SetFloat(ProgressProperty, 1.0f);
        
        go.MakeVisible();
        go.UnFreeze();
    }

    private void Update()
    {
        // If the animation is still running...
        // (it is completed when the `ProgressProperty` reaches 1.0f)
        if (_timer < 1.0f)
        {
            // advance the animation:
            _timer = Mathf.Min(1.0f, _timer + Time.deltaTime * _speed);
            _material.SetFloat(
                ProgressProperty,
                _isTeleportingOut ?
                    _timer :
                    1.0f - _timer // reverse the effect when teleporting an object in instead of out
            );
        }

        else
        {
            // Destroy this behaviour when the effect finished
            Destroy(this);
            
            // Make object permanently invisible, if this is the teleporting out effect
            if (_isTeleportingOut)
            {
                this.gameObject.MakeInvisible();
            }
        }
    }
}
