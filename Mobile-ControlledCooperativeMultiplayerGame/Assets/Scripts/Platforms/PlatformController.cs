using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public List<WayPointController> wayPoints;
    public Tuple<WayPointController, int> nextWayPoint;

    public PlayerController player;

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        nextWayPoint = Tuple.Create(wayPoints[0], 0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move() 
    {
        WayPointController wayPoint = nextWayPoint.Item1;

        if (isMoveHorizontalNeeded(wayPoint)) {
            MoveHorizontal();
        } 
        
        else if (isMoveVerticalNeeded(wayPoint)){
            MoveVertical();
        }

        else {
            updateNextWayPoint();
        }
    }

    private void updateNextWayPoint() 
    {
        if (areAllWayPointsVisited()) {
            wayPoints.Reverse();
            nextWayPoint = Tuple.Create(wayPoints[1], 1);
        } else {
            int nextIndex = nextWayPoint.Item2 + 1;
            nextWayPoint = Tuple.Create(wayPoints[nextIndex], nextIndex);
        }
    }

    private Boolean areAllWayPointsVisited()
    {
        return nextWayPoint.Item2 + 1 >= wayPoints.Count;
    }

    private Boolean isMoveVerticalNeeded(WayPointController wayPoint)
    {
        return Math.Abs(transform.position.y - wayPoint.transform.position.y) >= .9f;
    }

    private Boolean isMoveHorizontalNeeded(WayPointController wayPoint) 
    {
        return Math.Abs(transform.position.x - wayPoint.transform.position.x) >= .9f;
    }

    private void MoveHorizontal()
    {
        WayPointController wayPoint = nextWayPoint.Item1;
        Vector2 deltaPosition = transform.position;

        // Need to move Left
        if (wayPoint.transform.position.x < transform.position.x) {
            deltaPosition.x = deltaPosition.x - speed * Time.deltaTime;
        }
        // Need to move right
        else {
            deltaPosition.x = deltaPosition.x + speed * Time.deltaTime;
        }
        transform.position = deltaPosition;
    }

    private void MoveVertical()
    {
        WayPointController wayPoint = nextWayPoint.Item1;
        Vector2 deltaPosition = transform.position;

        // Need to move Down
        if (wayPoint.transform.position.y < transform.position.y) {
            deltaPosition.y = deltaPosition.y - speed * Time.deltaTime;
        }
        // Need to move Up
        else {
            deltaPosition.y = deltaPosition.y + speed * Time.deltaTime;
        }
        transform.position = deltaPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            other.GetComponent<PhysicsEffects>().SetCustomOrigin(this.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
            other.GetComponent<PhysicsEffects>().SetCustomOrigin(null);
        }
    }

}
