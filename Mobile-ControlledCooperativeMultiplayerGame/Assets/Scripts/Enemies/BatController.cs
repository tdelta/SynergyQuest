using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : EnemyController {
    [SerializeField] float flightSpeed = 1;
    [SerializeField] float flightChangeTime = 1;

    float flightTimer;
    int currentMotion = 0;
    readonly int moveYProperty = Animator.StringToHash("Move Y");
    Vector2[] flightMotions = new []{new Vector2(-1, 1), new Vector2(-1, -1),
        new Vector2(1, 1), new Vector2(1, -1)};

    protected override void Start() {
        base.Start();
        flightTimer = flightChangeTime;
    }

    protected override void Update() {
        base.Update();
        flightTimer -= Time.deltaTime;

        if (flightTimer < 0) {
            currentMotion = (currentMotion + 1) % 4;
            flightTimer = flightChangeTime;
        }
    }

    protected override Vector2 ComputeOffset() {
        var flightMotion = flightMotions[currentMotion];
        var offset = Time.deltaTime * directionSpeed * direction;
        offset += flightSpeed * Time.deltaTime * flightMotion;
        animator.SetFloat(moveYProperty, flightMotion.y);
        return offset;
    }
}
