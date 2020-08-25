using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerGhost : MonoBehaviour
{
    PlayerController _player;
    PlayerController _followedPlayer;
    Vector2 _respawnPosition;
    Animator _animator;
    Rigidbody2D _rigidbody2D;
    PhysicsEffects _physicsEffects;
    AudioSource _audioSource;
    System.Random _rnd = new System.Random();

    bool _moveArround;
    float RotateSpeed = 2f;
    float Radius = 0.175f;
    float _angle;

    float _playerChangeTime = 5;
    float _playerChangeTimer;

    static List<PlayerController> _otherPlayers;
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
        _otherPlayers = FindObjectsOfType<PlayerController>().ToList();
        _otherPlayers.Remove(_player);
    }

    public void Exorcise()
    {
        _animator.SetTrigger(VanishTrigger);
        _moveArround = false;
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
        _player.GetComponent<PhysicsEffects>().Teleport(_respawnPosition);
        // Make player visible again, if they have been invisible
        _player.GetComponent<SpriteRenderer>().enabled = true;
        _player.Reset();

        Destroy(transform.parent.gameObject);
    }

    PlayerController GetFollowedPlayer()
    {
        _playerChangeTimer -= Time.deltaTime;

        if (_playerChangeTimer < 0)
        {
            _followedPlayer = _otherPlayers[_rnd.Next(_otherPlayers.Count)];
            _playerChangeTimer = _playerChangeTime;
            // play sound when targeting a potentially new player
            _audioSource.Play();
        }

        return _followedPlayer;
    }

    void FixedUpdate()
    {

        // if we still have living players, move around, else respawn
        if (_otherPlayers.Any())
        {
            if (_moveArround)
            {
                _angle += RotateSpeed * Time.deltaTime;
       
                 var position = _rigidbody2D.position;
                 var rotation = new Vector2(Mathf.Sin(_angle), Mathf.Cos(_angle)) * Radius;
                 var direction = (GetFollowedPlayer().Center - position) * Time.deltaTime;

                 position += direction + rotation;
                 _physicsEffects.MoveBody(position);

                 _animator.SetFloat(MoveXProperty, direction.x);
            }        
        } else
            Exorcise();
    }
}
