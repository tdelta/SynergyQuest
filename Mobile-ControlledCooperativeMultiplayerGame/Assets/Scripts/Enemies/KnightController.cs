using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : EnemyController {
    [SerializeField] float viewCone = 1;

    readonly int moveXProperty = Animator.StringToHash("Move X");
    Vector2 offset = new Vector2(0, 0);

    (float, Vector2) FindNearestPlayer() {
        var players = (PlayerController[]) GameObject.FindObjectsOfType(typeof(PlayerController));
        Vector2 playerVector = new Vector2(0, 0);
        float playerAngle = 180;

        foreach (var player in players) {
            Vector2 position = player.GetPosition();
            Vector2 target = position - Rigidbody2D.position;
            float angle = Vector2.Angle(target, offset);

            if (angle < playerAngle) {
                playerAngle = angle;
                playerVector = target;
            }
        }

        return (playerAngle, playerVector.normalized);
    }

    protected override Vector2 ComputeOffset() {
        (var angle, var vector) = FindNearestPlayer();

        if (offset != Vector2.zero && angle <= viewCone / 2)
            offset = Time.deltaTime * directionSpeed * vector;
        else
            offset = Time.deltaTime * directionSpeed * direction;

        Animator.SetFloat(moveXProperty, offset.x);
        return offset;
    }
}