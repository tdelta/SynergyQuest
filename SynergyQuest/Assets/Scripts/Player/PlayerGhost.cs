// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

using System.Collections;
using UnityEngine;
using System.Linq;
using DamageSystem;

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
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PlayerGhost : MonoBehaviour
{
    // the player the ghost belongs to
    public PlayerController Player { get; private set; }

    // another active player the ghost currently follows
    private PlayerController _followedPlayer;

    private Vector2 _respawnPosition;
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private PhysicsEffects _physicsEffects;
    private AudioSource _audioSource;
    private Attackable _attackable;
    
    private System.Random _rnd = new System.Random();

    private bool _moveArround;
    private bool _exorcised;
    private float RotateSpeed = 1.75f;
    private float _angle;
    
    [SerializeField] float Radius = 0.125f;
    [SerializeField] private float minRadius = 0.03f;
    private float rMod = 1.0f;
    
    [SerializeField] private float approachSpeed = 0.2f;

    private float _playerChangeTime = 5;
    private float _playerChangeTimer;

    private readonly int MoveXProperty = Animator.StringToHash("Move X");
    private readonly int VanishTrigger = Animator.StringToHash("Vanish");

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
        _audioSource = GetComponent<AudioSource>();
        _attackable = GetComponent<Attackable>();
    }

    private void OnEnable()
    {
        _attackable.OnAttack += OnAttack;
    }
    
    private void OnDisable()
    {
        _attackable.OnAttack -= OnAttack;
    }

    public void Register(PlayerController player, Vector2 respawnPosition)
    {
        Player = player;
        _respawnPosition = respawnPosition;
        Player.gameObject.SetActive(false);
        Player.cameraTracked.Tracking = false;
        Player.Input.InputMode = InputMode.RevivalMinigame;
    }
    
    /**
     * <summary>
     * Restore the player this ghost was created from.
     * Currently, this is invoked by attacking this object, see <see cref="Attackable"/>.
     * </summary>
     */
    private void Exorcise()
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
        Player.cameraTracked.Tracking = true;
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

    /**
     * <summary>
     * Exorcise this ghost (<see cref="Exorcise"/>) if this object is attacked by a player.
     * Invoked by <see cref="Attackable"/>.
     * </summary>
     */
    private void OnAttack(AttackData attack)
    {
        if (attack.Attacker.CompareTag("Player"))
        {
            this.Exorcise();
        }
    }
}
