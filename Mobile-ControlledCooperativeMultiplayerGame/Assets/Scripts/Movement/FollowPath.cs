using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Makes an object follow a certain set of waypoints. It will try to reach them using direct linear movement.
 * It will traverse the waypoints in reverse direction after the last waypoint has been reached.
 * </summary>
 */
public class FollowPath : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints = new Transform[0];
    [SerializeField] private float speed = 1.0f;
    
    /**
     * Indicates, whether this object had already reached the last waypoint and is now following them in the reverse
     * direction
     */
    private int _direction = 1;
    /**
     * Index of the waypoint this object is currently trying to reach
     */
    private int _currentWaypointIdx = -1;
    /**
     * Position of the waypoint this object is currently trying to reach
     */
    private Vector3 _currentTargetPosition = Vector3.zero;

    private void Awake()
    {
        // If there are any waypoints, initialize the current movement target to the first waypoint
        if (waypoints.Any())
        {
            _currentWaypointIdx = 0;
            _currentTargetPosition = waypoints[_currentWaypointIdx].position;
        }
    }

    private void Update()
    {
        //Is there is a waypoint we are currently trying to reach?
        if (_currentWaypointIdx >= 0)
        {
            // Determine the distance we can travel in this frame
            var delta = Time.deltaTime * speed;
            
            // Move towards the target waypoint without overshooting it
            var newPosition = Vector3.MoveTowards(this.transform.position, _currentTargetPosition, delta);
            this.transform.position = newPosition;

            // If we have reached the target position...
            if (newPosition == _currentTargetPosition)
            {
                // Determine the index of the next waypoint to reach depending on the direction we are currently
                // traversing them
                _currentWaypointIdx += _direction;
                
                // If the index is beyond the last waypoint index...
                if (_currentWaypointIdx >= waypoints.Length)
                {
                    // ...we now traverse the waypoints in reverse direction
                    _direction = -1;
                    // and if there are at least 2 waypoints, the next waypoint is the one before the last one
                    if (waypoints.Length > 1)
                    {
                        _currentWaypointIdx = waypoints.Length - 2;
                    }

                    // otherwise, there is only 1 waypoint and we do not need to move anymore
                    else
                    {
                        _currentWaypointIdx = -1;
                    }
                }
                
                // If the index is below the first waypoint index...
                else if (_currentWaypointIdx < 0)
                {
                    // ...we traverse the waypoints in the normal direction again
                    _direction = 1;
                    // and if there are at least 2 waypoints, the next waypoint is the one after the first one
                    if (waypoints.Length > 1)
                    {
                        _currentWaypointIdx = 1;
                    }

                    // otherwise, there is only 1 waypoint and we do not need to move anymore
                    else
                    {
                        _currentWaypointIdx = -1;
                    }
                }

                _currentTargetPosition = waypoints[_currentWaypointIdx].position;
            }
        }
    }
}
