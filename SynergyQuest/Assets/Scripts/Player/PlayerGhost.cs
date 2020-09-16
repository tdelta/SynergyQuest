using System.Collections;
using UnityEngine;
using System.Linq;

/**
 * <summary>
 * Responsible to control the ghost of a player as part of the <see cref="ReviveMinigame"/>. The Exorcise method triggers
 * the respawning of a dead player
 * </summary>
 * <remarks>
 * The input mode of the player's controller is changed to <see cref="InputMode.RevivalMinigame"/>, which will display
 * information regarding the mini-game on the players controller.
 * </remarks>
 * <seealso cref="ReviveMinigame"/>
 */
public class PlayerGhost : MonoBehaviour
{
    // the player the ghost belongs to
    public PlayerController Player { get; private set; }

    // another active player the ghost currently follows
    PlayerController _followedPlayer;

    Vector2 _respawnPosition;
    Animator _animator;
    Rigidbody2D _rigidbody2D;
    PhysicsEffects _physicsEffects;
    AudioSource _audioSource;
    System.Random _rnd = new System.Random();

    bool _moveArround;
    bool _exorcised;
    float RotateSpeed = 1.75f;
    float _angle;
    
    [SerializeField] float Radius = 0.125f;
    [SerializeField] private float minRadius = 0.03f;
    private float rMod = 1.0f;
    
    [SerializeField] private float approachSpeed = 0.2f;

    float _playerChangeTime = 5;
    float _playerChangeTimer;

    readonly int MoveXProperty = Animator.StringToHash("Move X");
    readonly int VanishTrigger = Animator.StringToHash("Vanish");

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Register(PlayerController player, Vector2 respawnPosition)
    {
        Player = player;
        _respawnPosition = respawnPosition;
        Player.gameObject.SetActive(false);
        Player.gameObject.SetFollowedByCamera(false);
        Player.Input.InputMode = InputMode.RevivalMinigame;
    }

    public void Exorcise()
    {
        _animator.SetTrigger(VanishTrigger);
        _exorcised = true;
    }

    void OnAppear()
    {
        _moveArround = true;
        StartCoroutine(PeriodicallyApproach());
    }

    void OnVanish()
    {
        Player.gameObject.SetActive(true);
        Player.gameObject.SetFollowedByCamera(true);
        // Move player to position of the spawner when respawning
        Player.PhysicsEffects.Teleport(_respawnPosition);
        // Make player visible again, if they have been invisible
        Player.GetComponent<SpriteRenderer>().enabled = true;
        Player.OnRespawn(_respawnPosition, Spawnable.RespawnReason.Other);
        Player.Input.InputMode = InputMode.Normal;

        Destroy(transform.parent.gameObject);
    }

    PlayerController GetFollowedPlayer()
    {
        _playerChangeTimer -= Time.deltaTime;

        if (_playerChangeTimer < 0 || !(_followedPlayer?.gameObject.activeSelf ?? false))
        {
            _followedPlayer = null;
            _playerChangeTimer = _playerChangeTime;
            if (FindObjectsOfType<PlayerController>() is var activePlayers && activePlayers.Count() > 0)
            {
                _followedPlayer = activePlayers[_rnd.Next(activePlayers.Count())];
                // play sound when targeting a potentially new player
                _audioSource.Play();
            }
        }

        return _followedPlayer;
    }
    
    private IEnumerator StartApproach()
    {
        var targetMod = minRadius / Radius;
        while (rMod > targetMod)
        {
            rMod = Mathf.MoveTowards(rMod, targetMod, approachSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        rMod = 1.0f;
    }

    private IEnumerator PeriodicallyApproach()
    {
        while (!_exorcised)
        {
            yield return new WaitForSeconds(4.0f);
            yield return StartApproach();
        }
    }

    void FixedUpdate()
    {

        // if we still have living player to follow, move around, else exorcise/respawn
        if (!_exorcised && GetFollowedPlayer() is PlayerController followedPlayer)
        {
            if (_moveArround)
            {
                _angle += RotateSpeed * Time.deltaTime;
       
                 var position = _rigidbody2D.position;
                 var rotation = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * (Radius * rMod);
                 var direction = (followedPlayer.Center - position) * Time.deltaTime;

                 position += direction + rotation;
                 _physicsEffects.MoveBody(position);

                 _animator.SetFloat(MoveXProperty, direction.x);
            }        
        } else
            Exorcise();
    }
}
