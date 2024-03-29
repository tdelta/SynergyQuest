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

 using UnityEngine;

/**
 * A Pushable can either be resting or be moving
 */
public enum State
{
    Resting,
    Moving
}

/**
 * Models an object which can be pushed and pulled between different cells of a grid, e.g. a Sokoban box.
 */
public class Pushable : MonoBehaviour
{
    /**
     * A player must be at least this fast during a collision to be able to push this object
     */
    [SerializeField] private float minimumPushVelocity = 0.5f;
    /**
     * This object moves at this speed if pushed / pulled
     */
    [SerializeField] private float speed = 0.8f;
    /**
     * The player must be in contact with this object at least this long to be able to push / pull it
     */
    [SerializeField] private float inContactTime = 0.5f;
    /**
     * The player character must overlap this object on the relevant coordinate axis by this much to be able to push /
     * pull it. I.e. there must be enough contact surface. See als `BoundsExtensions.GetAxesOverlap()`.
     *
     * The idea here is that a player should not be able to push a box by walking only slightly against its corner.
     * Unit: Percent the contact surface overlaps the collision side
     */
    [SerializeField] private float minInteractionOverlap = 0.3f;
    /**
     * The grid in which this box can be moved. Usually you should set this to the grid of the tilemap.
     */
    [SerializeField] private Grid grid = default;

    [SerializeField] private AudioClip pushSound = default;

    /**
     * A Pushable should be part of an object with a box collider and a rigidbody.
     * These components are retrieved automatically during `Start`
     */
    private BoxCollider2D _boxCollider;
    private Rigidbody2D _body;
    private AudioSource _audioSource;

    /**
     * Records whether the box is currently moving or resting
     */
    public State state { get; private set; } = State.Resting;

    /**
     * If the box is moving, this records the distance it still has to move before stopping.
     */
    private float _remainingMoveDistance = 0.0f;
    /**
     * If the box is moving, this is the direction it is moving in.
     */
    private Vector2 _moveDirection = Vector2.zero;

    /**
     * We want to keep track of players who collided with this one until it has been in contact for at least
     * `inContactTime` since only after that time they may push this object.
     *
     * Hence we remember the object which is currently in contact in this field.
     */
    private PlayerController _inContactPlayer;
    
    /**
     * An `Interactive` behavior must be used in conjunction with this behavior to enable pulling.
     * We cache a reference to the player who is potentially pulling this object here.
     */
    private PlayerController _pullingPlayer;
    
    /**
     * Counts down from `inContactTime` to enable the functionality explained above
     */
    private float _inContactTimer;

    /**
     * Amount this object moves in x direction when pushed / pulled in x direction. Set in `Start`
     */
    private float _xMoveWidth = 0.0f;
    /**
     * Amount this object moves in y direction when pushed / pulled in y direction. Set in `Start`
     */
    private float _yMoveWidth = 0.0f;

    /**
     * This objects checks for obstacles when moving on these layers:
     */
    private static LayerMask _raycastLayerMask;
    
    /**
     * An `Interactive` behavior must be used in conjunction with this behavior to enable pulling.
     */
    private Interactive _interactive;

    /**
     * When a player pulls a box a joint is used to keep the box and the player connected.
     */
    private Joint2D _joint;
    

    // Start is called before the first frame update
    void Start()
    {
        if (grid == null)
        {
            grid = GameObject.Find("Grid").GetComponent<Grid>();
        }
        
        _raycastLayerMask  = LayerMask.GetMask("LevelStatic", "Box", "Chasm");
        
        _boxCollider = GetComponent<BoxCollider2D>();
        _body = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        _interactive = GetComponent<Interactive>();
        _joint = gameObject.AddComponent<FixedJoint2D>();
        
        // TODO: enabling collisions makes it possible to push boxes when in pulling state, is this desired?
        _joint.enableCollision = true;

        // We move by the size of a grid cell and the gaps between them, if present
        _xMoveWidth = grid.cellSize.x + grid.cellGap.x;
        _yMoveWidth = grid.cellSize.y + grid.cellGap.y;

        // Center this object in the cell that is around it.
        // This corrects small positioning errors made in the Unity Editor
        var cell = grid.WorldToCell(_body.position);
        _body.position = grid.GetCellCenterWorld(
            cell
        );
    }

    private void Update()
    {
        // If a player is interacting with this object by pressing the `Pull` button,
        // we cache a reference to it, and change its state accordingly to prepare it for pulling.
        if (_interactive.IsInteracting)
        {
            if (_pullingPlayer is null)
            {
                _pullingPlayer = _interactive.InteractingPlayer;
                _pullingPlayer.EnablePulling(this);
                _joint.connectedBody = _pullingPlayer.Rigidbody2D;
            }
        }
        
        // If a player has been interacting with this object using the `Pull` button, 
        // and the interaction stopped, we reset the players state and stop caching it.
        else if (state == State.Resting && !ReferenceEquals(_pullingPlayer, null))
        {
            _pullingPlayer.DisablePulling();
            _joint.connectedBody = null;
            _pullingPlayer = null;
        }
    }

    private void FixedUpdate()
    {
        // Do nothing here, if we are not moving.
        if (state != State.Moving) return;
        
        // Stop moving, if we have already the whole distance of one push / pull
        if (Mathf.Approximately(_remainingMoveDistance, 0))
        {
            state = State.Resting;
            // If the this object is no longer moving, we allow displaying interaction hints again
            _interactive.SuppressSpeechBubble = false;
            
            _audioSource.Stop();
        }

        // If we are moving, update our position
        else
        {
            // Move according to speed and time but not more than the maximum distance which must still be travelled
            var moveDistance = Mathf.Min(_remainingMoveDistance, speed * Time.deltaTime);
                
            _remainingMoveDistance -= moveDistance; // Reduce the distance we still need to travel
            
            _body.MovePosition(
                _body.position + _moveDirection * moveDistance
            );
        }
    }

    /**
     * Determines, whether this object can be pushed / pulled into a certain direction.
     * For example, a wall may be blocking the way or this object is already moving.
     *
     * @param directionVec              direction we would travel
     * @param moveDistance              distance we would travel
     * @param additionalRaycastDistance Additional distance beyond the moveDistance we check for obstacles (default = 0).
     *                                  This parameter is for example used to check the next 2 grid cells for obstacles
     *                                  when pulling, so that the player can not be crushed between this object and a
     *                                  wall.
     */
    private bool CanMove(Vector2 directionVec, float moveDistance, float additionalRaycastDistance = 0)
    {
        // We can only make another move if we are currently not moving:
        if (state == State.Resting)
        {
            // We now do a raycast to check for obstacles in the way of the move.
            var boxCenter = (Vector2) transform.position + _boxCollider.offset;
            var boxCastSize = _boxCollider.size - new Vector2(
                0.1f, // Allow for small tolerance, as floating point computations are not always that accurate
                0.1f
            );
            
            var hit = Physics2D.BoxCast(
                boxCenter,
                boxCastSize, // Raycast in the shape of the collider of this object.
                0,           // Don't rotate
                directionVec,
                moveDistance + additionalRaycastDistance,
                 _raycastLayerMask
            );
            var isSomethingInMoveDirection = !ReferenceEquals(hit.collider, null);

            return !isSomethingInMoveDirection;
        }

        else
        {
            return false;
        }
    }

    /**
     * Derive the distance we want to move from the direction.
     * 
     * I.e. if we are moving sideways, we move by the width of grid cells and if we
     * are moving vertically, we move by the height of grid cells.
     */
    private float GetMoveDistance(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
            case Direction.Right:
                return _xMoveWidth;
            case Direction.Up:
            case Direction.Down:
                return _yMoveWidth;
        }

        return 0;
    }

    /**
     * Can be called by characters to pull this object into a certain direction.
     * The pull will be performed over multiple (Fixed-)Update steps.
     *
     * @param direction     the direction to pull in
     * @param pullingPlayer the box will take care of moving the player while pulling. Hence the player game object
     *                      shall be passed here
     */
    public void Pull(Direction direction)
    {
        // We want to check the next 2 cells for obstacles, since otherwise a player might be crushed between this object
        // and a wall while pulling.
        // Hence we define additional raycasting distance.
        float additionalRayCastDistance = GetMoveDistance(direction);
        Move(direction, additionalRayCastDistance);
    }

    /**
     * Starts to move this object in a certain direction by the width of one grid cell if the path is not blocked.
     * The move is performed over multiple (Fixed-)Update steps.
     *
     * @param direction                 direction we are moving in
     * @param additionalRayCastDistance We will check the next cell for obstacles. If this parameter is set, we will
     *                                  further extend the raycast by the given length.
     * @returns whether this object started moving
     */
    private bool Move(Direction direction, float additionalRayCastDistance = 0)
    {
        var directionVec = direction.ToVector();
        var moveDistance = GetMoveDistance(direction);
        
        if (CanMove(directionVec, moveDistance, additionalRayCastDistance))
        {
            // See documentation of these fields for an explanation of why we are setting these values.
            state = State.Moving;
            // While the box is moving, we do not want to display any interaction hints to players
            _interactive.InteractingPlayer?.InteractionSpeechBubble.HideBubble();
            _interactive.SuppressSpeechBubble = true;
            
            _remainingMoveDistance = moveDistance;
            _moveDirection = directionVec;

            if (pushSound != null)
            {
                _audioSource.PlayOneShot(pushSound);
            }

            return true;
        }

        return false;
    }

    /**
     * We only want players to push / pull this object, if their contact surface if big enough that is, we don't want
     * players to push objects by a tiny corner.
     *
     * Hence, this method computes whether the contact surface is sufficient by projecting the player and this object
     * orthogonally onto the coordinate axes. Then the size of the overlap of these axis intervals is compared to the
     * size of this object. Iff it surpasses `minInteractionOverlap`, true is returned.
     *
     * @param interactionDirection direction in which the player is colliding with this object (i.e. the potential move direction)
     * @param other                usually the player that collided with this object.
     */
    bool ContactSurfaceSufficientToInteract(Direction interactionDirection, Collider2D other)
    {
        var thisBounds = _boxCollider.bounds;
        var otherBounds = other.bounds;

        var axisOverlap = thisBounds.GetAxesOverlap(otherBounds);

        float overlapLength = 0;
        float interactionSideLength = 1;
        
        switch (interactionDirection)
        {
            case Direction.Up:
            case Direction.Down:
                overlapLength = axisOverlap.x;
                interactionSideLength = _boxCollider.size.x;
                break;
            case Direction.Left:
            case Direction.Right:
                overlapLength = axisOverlap.y;
                interactionSideLength = _boxCollider.size.y;
                break;
        }
        
        return overlapLength / interactionSideLength > minInteractionOverlap;
    }

    /**
     * Called when an object keeps staying in contact with this one.
     */
    private void OnCollisionStay2D(Collision2D other)
    {
        // Abort if the object is not the first player who recently collided with us
        if (_inContactPlayer is null || other.gameObject != _inContactPlayer.gameObject) return;
        
        // Only continue if the player has already been in contact with this object for the minimum amount of time to
        // interact
        if (_inContactTimer <= 0.0f)
        {
            // The player must collide with this object with at least this velocity to be able to move it
            if (other.relativeVelocity.magnitude > minimumPushVelocity)
            {
                // The player must also be looking towards this object
                if ( _inContactPlayer.IsLookingAt(_boxCollider.bounds) )
                {
                    // The contact normal tells us, from what side the player is colliding with this object.
                    // We can derive the movement direction from it!
                    var moveVector = other.contacts[0].normal;
                    if (moveVector.ToDirection(out var moveDirection))
                    {
                        // Now, if there is enough contact surface between the player and this object, we can attempt to move
                        // (It may then still fail though since there may be an obstacle)
                        if (ContactSurfaceSufficientToInteract(moveDirection, other.collider))
                        {
                            Move(moveDirection);
                        }
                    }
                }
            }
        }

        // Otherwise, reduce the contact timer
        else
        {
            _inContactTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!ReferenceEquals(_inContactPlayer, null) &&  other.gameObject == _inContactPlayer.gameObject)
        {
            _inContactPlayer = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // If we are not currently at rest (i.e. moving), we ignore collisions
        if (state != State.Resting) return;
        // Abort, if we are already tracking a player
        if (!(_inContactPlayer is null)) return;
        // We also do not handle collisions with non-player objects
        if (!other.collider.CompareTag("Player")) return;
        // The color must also match
        if (!other.collider.GetComponent<PlayerController>().Color.IsCompatibleWith(_interactive.Color)) return;

        // A player collided with us!
        // We now need to track, how long it stays in contact with us.
        // (See also `OnCollisionStay2D`.)
        _inContactPlayer = other.gameObject.GetComponent<PlayerController>();
        _inContactTimer = inContactTime;
    }
    
    /**
     * There is a timeout before a player can push this object.
     * This method resets this timeout.
     */
    public void ResetContactTimeout()
    {
        _inContactTimer = inContactTime;
    }
}