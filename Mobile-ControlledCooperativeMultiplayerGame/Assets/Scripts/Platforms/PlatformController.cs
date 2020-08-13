using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private WayPointController[] wayPoints = new WayPointController[0];
    
    private const float EPSILON = 0.4f;

    private enum State
    {
        Idle,
        MovingX,
        MovingY
    }

    private State _state = State.Idle;
    private bool _reverseIdx = false;
    private Vector3 _nextPosition;
    private int _currentWaypointIdx = 0;
    private Vector2 _speed = Vector2.zero;

    private Collider2D _collider;

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (wayPoints.Any())
        {
            var startingPosition = CalculateTargetPosition(this.transform.position, 0);
            this.transform.position = startingPosition;
        }
        
        CalculateSpeed();
    }

    int GetNextWayPointIdx()
    {
        if (wayPoints.Length > 1)
        {
            if (_reverseIdx)
            {
                if (_currentWaypointIdx - 1 < 0)
                {
                    _reverseIdx = false;
                    return 1;
                }

                else
                {
                    return _currentWaypointIdx - 1;
                }
            }

            else
            {
                if (_currentWaypointIdx + 1 >= wayPoints.Length)
                {
                    _reverseIdx = true;
                    return wayPoints.Length - 1;
                }

                else
                {
                    return _currentWaypointIdx + 1;
                }
            }
        }

        else
        {
            return 0;
        }
    }

    void CalculateSpeed()
    {
        if (wayPoints.Any())
        {
            var nextWaypointIdx = GetNextWayPointIdx();
            
            var currentWaypoint = wayPoints[_currentWaypointIdx];
            var nextWaypoint = wayPoints[nextWaypointIdx];
            
            var currentTime = _currentWaypointIdx == 0 ? 0 : currentWaypoint.ArrivalTime;
            var travelingDuration = nextWaypoint.ArrivalTime - currentTime;
            
            var currentPosition = this.transform.position;
            var nextWayPointPosition = nextWaypoint.transform.position;

            var bounds = this._collider.bounds;
            _nextPosition = CalculateTargetPosition(currentPosition, nextWaypointIdx);
            
            if (!Mathf.Approximately(currentPosition.x, _nextPosition.x))
            {
                _state = State.MovingX;
                var distance = _nextPosition.x - currentPosition.x;
                
                _speed.x = distance / travelingDuration;
                _speed.y = 0;
            }

            else if (!Mathf.Approximately(currentPosition.y, _nextPosition.y))
            {
                _state = State.MovingY;
                var distance = _nextPosition.y - currentPosition.y;

                _speed.x = 0;
                _speed.y = distance / travelingDuration;
            }
            
            else if (_currentWaypointIdx != nextWaypointIdx)
            {
                _currentWaypointIdx = nextWaypointIdx;
                CalculateSpeed();
            }

            else
            {
                _state = State.Idle;
                _speed = Vector2.zero;
            }
        }

        else
        {
            _state = State.Idle;
            _speed = Vector2.zero;
        }
    }

    void CheckForSpeedUpdate()
    {
        if (
            _state is State.MovingX && Mathf.Approximately(this.transform.position.x, _nextPosition.x) ||
            _state is State.MovingY && Mathf.Approximately(this.transform.position.y, _nextPosition.y)
        )
        {
            CalculateSpeed();
        }
    }

    private void Update()
    {
        if (_state != State.Idle)
        {
            var currentPosition = this.transform.position;
            var positionDelta = _speed * Time.deltaTime;
            
            this.transform.position = new Vector3(
                Mathf.MoveTowards(currentPosition.x, _nextPosition.x, Mathf.Abs(positionDelta.x)),
                Mathf.MoveTowards(currentPosition.y, _nextPosition.y, Mathf.Abs(positionDelta.y)),
                currentPosition.z
            );
            
            CheckForSpeedUpdate();
        }
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

    private void OnDrawGizmos()
    {
        _collider = GetComponent<Collider2D>();
        var boundsSize = _collider.bounds.size;
        
        var currentPosition = this.transform.position;
        
        Gizmos.color = Color.red;
        for (int i = 0; i <= wayPoints.Length; ++i)
        {
            var targetPosition = CalculateTargetPosition(currentPosition, i % wayPoints.Length);
            Gizmos.DrawWireCube(
                targetPosition,
                boundsSize
            );

            if (i < wayPoints.Length)
            {
                Handles.Label(targetPosition, $"[{i}]@{wayPoints[i].ArrivalTime}s");
            }

            currentPosition = targetPosition;
        }
    }

    Vector3 CalculateTargetPosition(Vector3 startingPosition, int waypointIdx)
    {
        var waypointPosition = wayPoints[waypointIdx].transform.position;
        var bounds = _collider.bounds;

        var yEqual = MathfExtensions.Approximately(startingPosition.y, waypointPosition.y, EPSILON);
        var below = startingPosition.y < waypointPosition.y;
        var xEqual = MathfExtensions.Approximately(startingPosition.x, waypointPosition.x, EPSILON);
        var left = startingPosition.x < waypointPosition.x;
        
        return new Vector3(
            xEqual ?
                startingPosition.x :
                waypointPosition.x + 0.5f * bounds.size.x * (left ? -1 : 1),
            yEqual ?
                startingPosition.y :
                waypointPosition.y +0.5f * bounds.size.y * (below ? -1 : 1),
            startingPosition.z
        );
    }
}
