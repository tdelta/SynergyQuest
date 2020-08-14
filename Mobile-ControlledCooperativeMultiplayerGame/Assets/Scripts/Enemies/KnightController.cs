using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : EnemyController
{
    [SerializeField] float viewCone = 1;

    readonly int _moveXProperty = Animator.StringToHash("Move X");
    Vector2 _offset = new Vector2(0, 0);
    RaycastHit2D[] _hit = new RaycastHit2D[3];

    (bool, Vector2) FindNearestPlayer()
    {
        Vector2 playerVector = new Vector2(0, 0);
        float minPlayerDistance = Single.PositiveInfinity;

        foreach (var player in GameObject.FindObjectsOfType(typeof(PlayerController)) as PlayerController[])
        {
            Vector2 target = player.Center - Rigidbody2D.position;
            float distance = target.magnitude;

            if (Vector2.Angle(target, _offset) <= viewCone / 2 && distance < minPlayerDistance &&
                // if no gameObject blocks line of sight to player
                Physics2D.LinecastNonAlloc(Rigidbody2D.position, player.Center, _hit) == 2)
            {
                minPlayerDistance = distance;
                playerVector = target;
            }
        }

        return (!Single.IsInfinity(minPlayerDistance), playerVector.normalized);
    }

    protected override Vector2 ComputeOffset()
    {
        (var playerVisible, var playerDirection) = FindNearestPlayer();

        if (_offset != Vector2.zero && playerVisible)
            _offset = Time.deltaTime * directionSpeed * playerDirection;
        else
            _offset = Time.deltaTime * directionSpeed * direction;

        Animator.SetFloat(_moveXProperty, _offset.x);
        return _offset;
    }
}
