// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

 using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaypointControlledPlatform : MonoBehaviour
{
    [SerializeField] private List<WayPointController> wayPoints = new List<WayPointController>();
    [SerializeField] private float speed = default;
    /**
     * Time to wait when a endpoint has been reached before traveling the reverse direction
     */
    [SerializeField] private float endPointWaitTime = 0.0f;
    
    private bool _hasNextWaypoint = false;
    private (WayPointController, int) _nextWayPoint;

    // Start is called before the first frame update
    void Start()
    {
        if (wayPoints.Any())
        {
            _hasNextWaypoint = true;
            _nextWayPoint = (wayPoints[0], 0);
        }
        
        TriggerEndpointWait();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move() 
    {
        if (_hasNextWaypoint)
        {
            var wayPoint = _nextWayPoint.Item1;
            
            if (IsMoveHorizontalNeeded(wayPoint)) {
                MoveHorizontal();
            } 
            
            else if (IsMoveVerticalNeeded(wayPoint)){
                MoveVertical();
            }

            else {
                UpdateNextWayPoint();
            }
        }
    }

    private void UpdateNextWayPoint() 
    {
        if (AreAllWayPointsVisited())
        {
            wayPoints.Reverse();
            _nextWayPoint = (wayPoints[1], 1);
            
            TriggerEndpointWait();
        }
        
        else
        {
            int nextIndex = _nextWayPoint.Item2 + 1;
            _nextWayPoint = (wayPoints[nextIndex], nextIndex);
        }
    }

    private bool AreAllWayPointsVisited()
    {
        return _nextWayPoint.Item2 + 1 >= wayPoints.Count;
    }

    private bool IsMoveVerticalNeeded(WayPointController wayPoint)
    {
        return Math.Abs(transform.position.y - wayPoint.transform.position.y) > 0.0f;
    }

    private Boolean IsMoveHorizontalNeeded(WayPointController wayPoint) 
    {
        return Math.Abs(transform.position.x - wayPoint.transform.position.x) > 0.0f;
    }

    private void MoveHorizontal()
    {
        WayPointController wayPoint = _nextWayPoint.Item1;
        Vector3 deltaPosition = transform.position;

        // Need to move Left
        if (wayPoint.transform.position.x < transform.position.x) {
            deltaPosition.x = Mathf.Max(deltaPosition.x - speed * Time.deltaTime, wayPoint.transform.position.x);
        }
        // Need to move right
        else {
            deltaPosition.x = Mathf.Min(deltaPosition.x + speed * Time.deltaTime, wayPoint.transform.position.x);
        }
        
        transform.position = deltaPosition;
    }

    private void MoveVertical()
    {
        WayPointController wayPoint = _nextWayPoint.Item1;
        Vector3 deltaPosition = transform.position;

        // Need to move Down
        if (wayPoint.transform.position.y < transform.position.y) {
            deltaPosition.y = Mathf.Max(deltaPosition.y - speed * Time.deltaTime, wayPoint.transform.position.y);
        }
        // Need to move Up
        else {
            deltaPosition.y = Mathf.Min(deltaPosition.y + speed * Time.deltaTime, wayPoint.transform.position.y);
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

    /**
     * <summary>
     * Causes platform to delay movement for `endPointWaitTime`.
     * </summary>
     */
    private void TriggerEndpointWait()
    {
        if (endPointWaitTime > 0)
        {
            var originalSpeed = speed;
            speed = 0.0f;
            StartCoroutine(CoroutineUtils.Wait(endPointWaitTime, () =>
            {
                speed = originalSpeed;
            }));
        }
    }
}
