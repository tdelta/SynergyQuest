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

using DamageSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Attackable))]
public class NecromancerController : EnemyController
{
    [FormerlySerializedAs("fireball")]
    [SerializeField]
    private FireballProjectile fireballPrefab = default;
    [SerializeField] private float launchCoolDown = 1;
    [SerializeField] private float viewCone = 1;

    private readonly int _moveXProperty = Animator.StringToHash("Move X");
    private float _launchTimer;
    private Vector2 _offset = new Vector2(0, 0);
    private RaycastHit2D[] _hit = new RaycastHit2D[3];
    private PlayerColor _currentColor = PlayerColor.Any;
    [CanBeNull] private PlayerController _lastAttackingPlayer;

    private static readonly int CapeColor = Shader.PropertyToID("_CapeColor");

    private void OnEnable()
    {
        attackable.OnPendingAttack += OnPendingAttack;
        attackable.OnAttack += OnAttack;
    }
    
    private void OnDisable()
    {
        attackable.OnPendingAttack -= OnPendingAttack;
        attackable.OnAttack -= OnAttack;
    }

    protected override void Start()
    {
        base.Start();
        _launchTimer = launchCoolDown;
    }

    (bool, Vector2) IsPlayerVisible()
    {
        if (_lastAttackingPlayer != null)
        {
            var target = _lastAttackingPlayer.Center - Rigidbody2D.position;
            // if no gameObject blocks line of sight to player
            if (Physics2D.LinecastNonAlloc(Rigidbody2D.position, _lastAttackingPlayer.Center, _hit) == 2)
                return (true, target.normalized);
        }

        return (false, Vector2.zero);
    }

    void LaunchFireball(Vector2 direction)
    {
        _launchTimer -= Time.deltaTime;

        if (_launchTimer < 0)
        {
            var spawnPoint = direction + Rigidbody2D.position;
            
            var instance = FireballProjectile.Launch(this.gameObject, fireballPrefab, spawnPoint, direction);
            Physics2D.IgnoreCollision(instance.Collider, GetComponent<Collider2D>());
            
            _launchTimer = launchCoolDown;
        }
    }

    protected override Vector2 ComputeOffset()
    {
        var (playerVisible, playerDirection) = IsPlayerVisible();
        if (!playerVisible)
          (playerVisible, playerDirection) = FindNearestPlayer(_offset, viewCone);

        if (playerVisible)
        {
            LaunchFireball(playerDirection);
            _offset = Time.deltaTime * directionSpeed * playerDirection;
        }
        else
            _offset = Time.deltaTime * directionSpeed * direction;

        Animator.SetFloat(_moveXProperty, _offset.x);
        return _offset;
    }

    /**
     * <summary>
     * Cancels incoming attacks if they come from a player with a color incompatible with the current cape color.
     * Invoked by <see cref="Attackable"/>.
     * </summary>
     */
    private void OnPendingAttack(AttackData attack)
    {
        // If the attacker is a player of incompatible color, cancel attack
        if (attack.attacker.TryGetComponent(out PlayerController attackingPlayer))
        {
            if (!_currentColor.IsCompatibleWith(attackingPlayer.Color))
            {
                attack.damage = 0;
                attack.knockback = 0;
                
                // also knock back the attacker a bit
                if (attackingPlayer.TryGetComponent(out Attackable playerAttackable))
                {
                    playerAttackable.Attack(new AttackData
                    {
                        attacker = this.gameObject,
                        knockback = 2,
                        attackDirection = attack.attackDirection.Map(attackDir=> -attackDir)
                    });
                }
            }
        }
    }

    /**
     * <summary>
     * If an attack happens on this object, changes the current cape color to the next one.
     * Invoked by <see cref="Attackable"/>.
     * </summary>
     */
    private void OnAttack(AttackData attack)
    {
        if (attack.attacker.TryGetComponent(out PlayerController attackingPlayer))
        {
            _lastAttackingPlayer = attackingPlayer;
            
            var material = GetComponent<Renderer>().material;
            
            _currentColor = attackingPlayer.Color.NextColor(PlayerDataKeeper.Instance.NumPlayers);
            
            material.SetColor(CapeColor, _currentColor.ToRGB());
        }
    }
}
