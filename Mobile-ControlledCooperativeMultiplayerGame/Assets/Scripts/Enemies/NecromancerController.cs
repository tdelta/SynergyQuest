using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecromancerController : EnemyController
{
    readonly int _moveXProperty = Animator.StringToHash("Move X");
    Vector2 _offset = new Vector2(0, 0);
    RaycastHit2D[] _hit = new RaycastHit2D[3];
    PlayerColor _currentColor = PlayerColor.Any;
    PlayerController _attackingPlayer;
    bool _attackedByPlayer = false;

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

    protected override Vector2 ComputeOffset()
    {
        (var playerVisible, var playerDirection) = IsPlayerVisible();

        if (playerVisible)
            _offset = Time.deltaTime * directionSpeed * playerDirection;
        else
            _offset = Time.deltaTime * directionSpeed * direction;

        Animator.SetFloat(_moveXProperty, _offset.x);
        return _offset;
    }

    protected override bool ChangeHealth(int amount, bool playSounds = true)
    {
        // if we didn't receive damage from a player apply damage regularly
        if (!_attackedByPlayer)
            return base.ChangeHealth(amount, playSounds);
        // if we received damage from a player apply damage if playercolor is compatible
        else
        {
            _attackedByPlayer = false;
            if (PlayerColorMethods.IsCompatibleWith(_currentColor, _attackingPlayer.Color))
            {
                var material = GetComponent<Renderer>().material;
                _currentColor = PlayerColorMethods.NextColor(_attackingPlayer.Color,
                    PlayerDataKeeper.Instance.NumPlayers);
                material.SetColor("_CapeColor", PlayerColorMethods.ColorToRGB(_currentColor));

                return base.ChangeHealth(amount, playSounds);
            }
        }

        return false;
    }
}
