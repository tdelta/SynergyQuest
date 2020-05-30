using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : MonoBehaviour {
    public float directionSpeed = 1;
    public float directionChangeTime = 1;
    public float flightSpeed = 1;
    public float flightChangeTime = 1;
    

    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private float directionTimer;
    private Vector2 direction;
    private float changeNoiseTime;
    private float flightTimer;
    private Vector2[] flightMotions = new []{new Vector2(-1, 1), new Vector2(-1, -1),
        new Vector2(1, 1), new Vector2(1, -1)};
    private int currentMotion = 0;
    
    void Start() {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        direction = Random.insideUnitCircle.normalized;
        directionTimer = directionChangeTime;
        flightTimer = flightChangeTime;
    }

    void Update() {
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

    Vector2 getNewOffset() {
        var flightMotion = flightMotions[currentMotion];
        var offset = Time.deltaTime * directionSpeed * direction;
        offset += flightSpeed * flightMotion;
        animator.SetFloat("Move Y", flightMotion.y);
        return offset;
    }

    void FixedUpdate() {
        Vector2 position = rigidbody2D.position;
        position += getNewOffset();
        rigidbody2D.MovePosition(position);
    }

        void OnCollisionEnter2D(Collision2D other) {
        direction = -direction;
    }
}
