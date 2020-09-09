using System.Collections;
using System.Collections.Generic;
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
    PlayerController _player;
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
    float Radius = 0.125f;
    float _angle;

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
        _player = player;
        _respawnPosition = respawnPosition;
        _player.gameObject.SetActive(false);
        _player.gameObject.SetFollowedByCamera(false);
        _player.Input.InputMode = InputMode.RevivalMinigame;
    }

    public void Exorcise()
    {
        _animator.SetTrigger(VanishTrigger);
        _exorcised = true;
    }

    void OnAppear()
    {
        _moveArround = true;
    }

    void OnVanish()
    {
        _player.gameObject.SetActive(true);
        _player.gameObject.SetFollowedByCamera(true);
        // Move player to position of the spawner when respawning
        _player.PhysicsEffects.Teleport(_respawnPosition);
        // Make player visible again, if they have been invisible
        _player.GetComponent<SpriteRenderer>().enabled = true;
        _player.OnRespawn(_respawnPosition, Spawnable.RespawnReason.Other);
        _player.Input.InputMode = InputMode.Normal;

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

    void FixedUpdate()
    {

        // if we still have living player to follow, move around, else exorcise/respawn
        if (!_exorcised && GetFollowedPlayer() is PlayerController followedPlayer)
        {
            if (_moveArround)
            {
                _angle += RotateSpeed * Time.deltaTime;
       
                 var position = _rigidbody2D.position;
                 var rotation = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * Radius;
                 var direction = (followedPlayer.Center - position) * Time.deltaTime;

                 position += direction + rotation;
                 _physicsEffects.MoveBody(position);

                 _animator.SetFloat(MoveXProperty, direction.x);
            }        
        } else
            Exorcise();
    }
}
