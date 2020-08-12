using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecromancerController : EnemyController {
    [SerializeField] float viewCone = 1;

    readonly int _moveXProperty = Animator.StringToHash("Move X");
    Vector2 _offset = new Vector2(0, 0);
    PlayerColor _currentColor = PlayerColor.Any;
    PlayerColor? _colorCandidate;

    public PlayerColor ColorCandidate
    {
        set => _colorCandidate = value;
    }

    (float, Vector2) FindNearestPlayer() {
        var players = (PlayerController[]) GameObject.FindObjectsOfType(typeof(PlayerController));
        Vector2 playerVector = new Vector2(0, 0);
        float playerAngle = 180;

        foreach (var player in players) {
            Vector2 position = player.GetPosition();
            Vector2 target = position - Rigidbody2D.position;
            float angle = Vector2.Angle(target, _offset);

            if (angle < playerAngle) {
                playerAngle = angle;
                playerVector = target;
            }
        }

        return (playerAngle, playerVector.normalized);
    }

    protected override Vector2 ComputeOffset() {
        (var angle, var vector) = FindNearestPlayer();

        if (_offset != Vector2.zero && angle <= viewCone / 2)
            _offset = Time.deltaTime * directionSpeed * vector;
        else
            _offset = Time.deltaTime * directionSpeed * direction;

        Animator.SetFloat(_moveXProperty, _offset.x);
        return _offset;
    }

    protected override bool ChangeHealth(int amount, bool playSounds = true)
    {
        if (_colorCandidate == null)
            return base.ChangeHealth(amount, playSounds);
        else if (PlayerColorMethods.IsCompatibleWith(_currentColor, _colorCandidate.Value))
        {
            var material = GetComponent<Renderer>().material;
            _currentColor = PlayerColorMethods.NextColor(_colorCandidate.Value, PlayerDataKeeper.Instance.NumPlayers);
            material.SetColor("_CapeColor", PlayerColorMethods.ColorToRGB(_currentColor));

            _colorCandidate = null;
            return base.ChangeHealth(amount, playSounds);
        }

        _colorCandidate = null;
        return false;
    }
}
