using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerGhost : MonoBehaviour
{
    PlayerController _player;
    Vector2 _respawnPosition;
    Animator _animator;
    Rigidbody2D _rigidbody2D;
    PhysicsEffects _physicsEffects;

    static List<PlayerController> _otherPlayers;
    readonly int MoveXProperty = Animator.StringToHash("Move X");
    readonly int VanishTrigger = Animator.StringToHash("Vanish");

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
        _otherPlayers = _otherPlayers ?? FindObjectsOfType<PlayerController>().ToList();
    }

    public void Register(PlayerController player, Vector2 respawnPosition)
    {
        _player = player;
        _respawnPosition = respawnPosition;
        _player.gameObject.SetActive(false);
        _otherPlayers.Remove(_player);
    }

    public void Exorcise()
    {
        _animator.SetTrigger(VanishTrigger);
    }

    public void OnVanish()
    {
        _player.gameObject.SetActive(true);
        // Move player to position of the spawner when respawning
        _player.GetComponent<PhysicsEffects>().Teleport(_respawnPosition);
        // Make player visible again, if they have been invisible
        _player.GetComponent<SpriteRenderer>().enabled = true;
        _player.Reset();

        Destroy(transform.parent.gameObject);
    }

    void FixedUpdate()
    {
        // if we still have living players, move around, else respawn
        if (_otherPlayers.Any())
        {
            var direction = _otherPlayers.First().Center - _rigidbody2D.position;
            var position = _rigidbody2D.position;

            position += direction.normalized * Time.deltaTime;
            _physicsEffects.MoveBody(position);

            _animator.SetFloat(MoveXProperty, direction.x);
        } else
          Exorcise();
    }
}
