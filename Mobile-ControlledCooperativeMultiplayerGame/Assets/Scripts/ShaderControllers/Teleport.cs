using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Teleport : MonoBehaviour
{
    private float _speed = 1.0f;
    private float _timer = 0.0f;
    private bool _isTeleportingOut;

    private Material _material;
    
    private static readonly int ColorProperty = Shader.PropertyToID("_TeleportColor");
    private static readonly int ProgressProperty = Shader.PropertyToID("_TeleportProgress");
    
    void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;

        if (!_material.HasProperty(ColorProperty) || !_material.HasProperty(ProgressProperty))
        {
            Debug.LogError("Trying to apply teleport effect to sprite with a material which does not support teleport effects.");
        }
    }

    public static void TeleportOut(GameObject go, Color color, float speed = 0.75f)
    {
        go.Freeze();
    
        var instance = go.AddComponent<Teleport>();
        instance._speed = speed;
        instance._isTeleportingOut = true;
        instance._material.SetColor(ColorProperty, color);
        instance._material.SetFloat(ProgressProperty, 0.0f);
    }
    
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
        if (_timer < 1.0f)
        {
            _timer = Mathf.Min(1.0f, _timer + Time.deltaTime * _speed);
            _material.SetFloat(ProgressProperty, _isTeleportingOut ? _timer : 1.0f - _timer);
        }

        else
        {
            Destroy(this);
            if (_isTeleportingOut)
            {
                this.gameObject.MakeInvisible();
            }
        }
    }
}
