using UnityEngine;

public class ButtonController: MonoBehaviour
{

  [SerializeField] private Sprite UnpressedButton = default;
  [SerializeField] private Sprite PressedButton = default;
  [SerializeField] private Pressable Effect = default;

  private bool _pressAnimation;
  private SpriteRenderer _renderer;
  private float _animationTime;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = UnpressedButton;
    }

    void Update()
    {
      if (_pressAnimation)
      {
        _animationTime -= Time.deltaTime;
        if (_animationTime <= 0)
        {
          _renderer.sprite = UnpressedButton;
          _pressAnimation = false;
          _animationTime = 1f;
        }
      }
    }

    public void OnPress()
    {
        _renderer.sprite = PressedButton;
        _pressAnimation = true;
        Effect.OnButtonPressed();
    }
}
