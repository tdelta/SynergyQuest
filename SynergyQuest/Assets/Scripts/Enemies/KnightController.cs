using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : EnemyController
{
    [SerializeField] float viewCone = 1;

    readonly int _moveXProperty = Animator.StringToHash("Move X");
    Vector2 _offset = new Vector2(0, 0);

    protected override Vector2 ComputeOffset()
    {
        (var playerVisible, var playerDirection) = FindNearestPlayer(_offset, viewCone);

        if (_offset != Vector2.zero && playerVisible)
            _offset = Time.deltaTime * directionSpeed * playerDirection;
        else
            _offset = Time.deltaTime * directionSpeed * direction;

        Animator.SetFloat(_moveXProperty, _offset.x);
        return _offset;
    }
}
