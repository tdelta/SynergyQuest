using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : EnemyController {
    public float directionSpeed = 1;
    public float directionChangeTime = 1;

    private float directionTimer;
    private Vector2 direction;
    
    protected override void Start() {
        base.Start();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
    }

    protected override void Update() {
        base.Update();
        directionTimer -= Time.deltaTime;

        if (directionTimer < 0) {
            direction = Random.insideUnitCircle.normalized;
            directionTimer = directionChangeTime;
        }

    }

    Vector2 computeNewOffset() {
        var offset = Time.deltaTime * directionSpeed * direction;
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
