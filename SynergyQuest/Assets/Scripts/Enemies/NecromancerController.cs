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
using UnityEngine;
using UnityEngine.Serialization;

public class NecromancerController : EnemyController, AttackInhibitor
{
    [FormerlySerializedAs("fireball")] [SerializeField] FireballProjectile fireballPrefab = default;
    [SerializeField] float launchCoolDown = 1;
    [SerializeField] float viewCone = 1;

    readonly int _moveXProperty = Animator.StringToHash("Move X");
    float _launchTimer;
    Vector2 _offset = new Vector2(0, 0);
    RaycastHit2D[] _hit = new RaycastHit2D[3];
    PlayerColor _currentColor = PlayerColor.Any;
    PlayerController _attackingPlayer;
    bool _attackedByPlayer = false;

    protected override void Start()
    {
        base.Start();
        _launchTimer = launchCoolDown;
    }

    public PlayerController AttackingPlayer
    {
        set
        {
            _attackedByPlayer = true;
            _attackingPlayer = value;
        }
    }

    (bool, Vector2) IsPlayerVisible()
    {
        if (_attackingPlayer != null)
        {
            var target = _attackingPlayer.Center - Rigidbody2D.position;
            // if no gameObject blocks line of sight to player
            if (Physics2D.LinecastNonAlloc(Rigidbody2D.position, _attackingPlayer.Center, _hit) == 2)
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
            
            var instance = FireballProjectile.Launch(fireballPrefab, spawnPoint, direction);
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

    public override bool ChangeHealth(int amount, bool playSounds = true)
    {
        // if we didn't receive damage from a player apply damage regularly
        if (!_attackedByPlayer)
            return base.ChangeHealth(amount, playSounds);
        // if we received damage from a player apply damage if playercolor is compatible
        else
        {
            _attackedByPlayer = false;
            if (_currentColor.IsCompatibleWith(_attackingPlayer.Color))
            {
                var material = GetComponent<Renderer>().material;
                _currentColor = PlayerColorMethods.NextColor(_attackingPlayer.Color,
                    PlayerDataKeeper.Instance.NumPlayers);
                material.SetColor("_CapeColor", _currentColor.ToRGB());

                return base.ChangeHealth(amount, playSounds);
            }
        }

        return false;
    }

    public bool IsAttackSuccessful(GameObject attacker)
    {
        if (attacker.TryGetComponent(out PlayerController _attackingPlayer))
        {
            return _currentColor.IsCompatibleWith(_attackingPlayer.Color);
        }

        return true;
    }
}
