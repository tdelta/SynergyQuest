using UnityEngine;

public class ButtonController: MonoBehaviour
{

  [SerializeField] private Sprite UnpressedButton;
  [SerializeField] private Sprite PressedButton;
  [SerializeField] private Pressable Effect;

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

    void OnPress()
    {
        _renderer.sprite = PressedButton;
        _pressAnimation = true;
        Effect.OnButtonPressed();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            var player = other.gameObject.GetComponent<PlayerController>();
            player.EnableGameAction(Button.Press);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            var player = other.gameObject.GetComponent<PlayerController>();
            player.DisableGameAction(Button.Press);
        }
    }
}
