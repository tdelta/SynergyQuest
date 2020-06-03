﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : EnemyController {
    public float flightSpeed = 1;
    public float flightChangeTime = 1;

    float flightTimer;
    int currentMotion = 0;
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

    protected override Vector2 computeForce() {
        var flightMotion = flightMotions[currentMotion];
        rigidbody2D.position += Time.fixedDeltaTime * flightSpeed * flightMotion;
        animator.SetFloat("Move Y", flightMotion.y);
        return directionSpeed * direction;
    }
}
