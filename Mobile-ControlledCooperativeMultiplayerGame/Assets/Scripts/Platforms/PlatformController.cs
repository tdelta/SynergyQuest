using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public List<WayPointController> wayPoints;
    public (WayPointController, int) nextWayPoint;

    public PlayerController player;

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        nextWayPoint = (wayPoints[0], 0);
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
        
        else if (IsMoveVerticalNeeded(wayPoint)){
            MoveVertical();
        }

        else {
            UpdateNextWayPoint();
        }
    }

    private void UpdateNextWayPoint() 
    {
        if (AreAllWayPointsVisited())
        {
            wayPoints.Reverse();
            nextWayPoint = (wayPoints[1], 1);
        }
        
        else
        {
            int nextIndex = nextWayPoint.Item2 + 1;
            nextWayPoint = (wayPoints[nextIndex], nextIndex);
        }
    }

    private bool AreAllWayPointsVisited()
    {
        return nextWayPoint.Item2 + 1 >= wayPoints.Count;
    }

    private bool IsMoveVerticalNeeded(WayPointController wayPoint)
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
        Vector3 deltaPosition = transform.position;

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
        Vector3 deltaPosition = transform.position;

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
        // Only transport an object, if it has a `PlatformTransportable` component
        var maybePlatformTransportable = other.GetComponent<PlatformTransportable>();
        if (maybePlatformTransportable != null)
        {
            // If the object has a physics effects component, we set the platform as new origin which it from now on
            // shall move relative to
            var maybePhysicsEffects = other.GetComponent<PhysicsEffects>();
            if (maybePhysicsEffects != null)
            {
                maybePhysicsEffects.SetCustomOrigin(this.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Only handle objects, if they have a `PlatformTransportable` component
        var maybePlatformTransportable = other.GetComponent<PlatformTransportable>();
        if (maybePlatformTransportable != null)
        {
            // If the object has a physics effects component...
            var maybePhysicsEffects = other.GetComponent<PhysicsEffects>();
            if (maybePhysicsEffects != null)
            {
                // ...and it is using this platform as custom origin to move relative to, then remove the custom origin,
                // since the object has left the platform
                if (maybePhysicsEffects.CustomOrigin == this.transform)
                {
                    maybePhysicsEffects.RemoveCustomOrigin();
                }
            }
        }
    }

}
