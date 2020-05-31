using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : EnemyController {
    public float directionSpeed = 1;
    public float directionChangeTime = 1;
    public float flightSpeed = 1;
    public float flightChangeTime = 1;

    private float directionTimer;
    private Vector2 direction;
    private float flightTimer;
    private Vector2[] flightMotions = new []{new Vector2(-1, 1), new Vector2(-1, -1),
        new Vector2(1, 1), new Vector2(1, -1)};
    private int currentMotion = 0;
    
    protected override void Start() {
        base.Start();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
        flightTimer = flightChangeTime;
    }

    protected override void Update() {
        base.Update();
        directionTimer -= Time.deltaTime;
        flightTimer -= Time.deltaTime;

        if (directionTimer < 0) {
            direction = Random.insideUnitCircle.normalized;
            directionTimer = directionChangeTime;
        }

        if (flightTimer < 0) {
            currentMotion = (currentMotion + 1) % 4;
            flightTimer = flightChangeTime;
        }
    }

    Vector2 computeNewOffset() {
        var flightMotion = flightMotions[currentMotion];
        var offset = Time.deltaTime * directionSpeed * direction;
        offset += flightSpeed * Time.deltaTime * flightMotion;
        animator.SetFloat("Move Y", flightMotion.y);
        return offset;
    }

    protected override void FixedUpdate() {
        if (!isDead) {
            Vector2 position = rigidbody2D.position;
            position += computeNewOffset();
            rigidbody2D.MovePosition(position);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D other) {
        base.OnCollisionEnter2D(other);
        direction = -direction;
    }
}
