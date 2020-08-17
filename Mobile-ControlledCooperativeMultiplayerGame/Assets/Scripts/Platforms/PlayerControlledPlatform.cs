using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(ColorReplacer), typeof(Switchable), typeof(Rigidbody2D))]
public class PlayerControlledPlatform : MonoBehaviour
{
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private ColorSwitch colorSwitch;
    
    private Switchable _switchable;
    private Rigidbody2D _body;
    private ColorReplacer _replacer;
    private Collider2D _collider;

    private PlayerController _player;
    private Color _originalColor;

    public PlayerController Player
    {
        get => _player;
        set
        {
            if (!ReferenceEquals(_player, null))
            {
                _player.Input.InputMode = InputMode.Normal;
                _player.Input.EnableButtons((Button.Exit, false));
            }
            _player = value;
            if (!ReferenceEquals(_player, null))
            {
                _player.Input.InputMode = InputMode.IMUOrientation;
                _player.Input.EnableButtons((Button.Exit, true));
            }
            
            // ReSharper disable once Unity.NoNullPropagation
            _replacer.ReplacementColor = value?.Color.ToRGB() ?? _originalColor;
        }
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _replacer = GetComponent<ColorReplacer>();
        _switchable = GetComponent<Switchable>();
        _collider = GetComponent<Collider2D>();

        _switchable.AddSwitches(colorSwitch.GetComponent<Switch>());

        _originalColor = _replacer.ReplacementColor;
    }
    
    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Update()
    {
        if (!ReferenceEquals(Player, null))
        {
            var delta = new Vector2(
                Player.Input.GetIMUOrientationHorizontal(),
                Player.Input.GetIMUOrientationVertical()
            ) * (speed * Time.deltaTime);
            
            _body.MovePosition(_body.position + delta);

            if (Player.Input.GetButtonDown(Button.Exit))
            {
                _switchable.enabled = true;
                Player.PhysicsEffects.Teleport2D(colorSwitch.transform.position);
                Teleport.TeleportIn(Player.gameObject, Player.Color.ToRGB());
                Teleport.TeleportIn(colorSwitch.gameObject, colorSwitch.Color.ToRGB());
                Player = null;
                
                foreach (var cameraTargetGroup in FindObjectsOfType<CinemachineTargetGroup>())
                {
                    var bounds = _collider.bounds;
                    cameraTargetGroup.RemoveMember(this.transform);
                }
            }
        }
    }
    
    void OnActivationChanged(bool activation)
    {
        if (activation)
        {
            if (!ReferenceEquals(colorSwitch.ActivatingPlayer, null))
            {
                _switchable.enabled = false;
                
                Player = colorSwitch.ActivatingPlayer;
                
                Teleport.TeleportOut(Player.gameObject, Player.Color.ToRGB());
                Teleport.TeleportOut(colorSwitch.gameObject, colorSwitch.Color.ToRGB());

                foreach (var cameraTargetGroup in FindObjectsOfType<CinemachineTargetGroup>())
                {
                    var bounds = _collider.bounds;
                    cameraTargetGroup.AddMember(
                        this.transform,
                        1,
                        Mathf.Max(bounds.extents.x, bounds.extents.y)
                    );
                }
            }
        }
    }
}
