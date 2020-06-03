using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightController : EnemyController {
    public float viewCone = 1;

    Vector2 offset = new Vector2(0, 0);

    (float, Vector2) findNearestPlayer() {
        var players = (PlayerController[]) GameObject.FindObjectsOfType(typeof(PlayerController));
        Vector2 playerVector = new Vector2(0, 0);
        float playerAngle = 180;

        foreach (var player in players) {
            Vector2 position = player.getPosition();
            Vector2 target = position - rigidbody2D.position;
            float angle = Vector2.Angle(target, offset);

            if (angle < playerAngle) {
                playerAngle = angle;
                playerVector = target;
            }
        }

        return (playerAngle, playerVector.normalized);
    }

    protected override Vector2 computeForce() {
        (var angle, var vector) = findNearestPlayer();

        if (offset != Vector2.zero && angle <= viewCone / 2)
            offset = directionSpeed * vector;
        else
            offset = directionSpeed * direction;

        animator.SetFloat("Move X", offset.x);
        return offset;
    }
}