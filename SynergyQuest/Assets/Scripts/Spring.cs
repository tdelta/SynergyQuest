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

using System.Collections;
using UnityEngine;

/**
 * <summary>
 * Interactive object which allows players to be transported to any map destination.
 * Visually, it animates a player jumping to the target location using a spring.
 * </summary>
 * <seealso cref="Interactive"/>
 */
[RequireComponent(typeof(Interactive), typeof(Animator), typeof(AudioSource))]
public class Spring : MonoBehaviour
{
    [Tooltip("Location where the jump shall start. Preferably a location on top of the spring sprite.")]
    [SerializeField] private Transform springJumpStartPoint = default;
    
    [Tooltip("Location where players using the spring will be transported.")]
    [SerializeField] private Transform springJumpDestination = default;

    [Tooltip("How fast the player will be moved to the jump start point when triggering the Spring")]
    [SerializeField] private float jumpOnSpringSpeed = 3.0f;
    
    [Tooltip("How long the spring is animated to tense up before initiating the jump.")]
    [SerializeField] private float underTensionTime = 1.0f; 
    
    [Tooltip("How fast the player will move when jumping using the spring.")]
    [SerializeField] private float jumpSpeed = 3.0f;
    
    [Tooltip("How much the player will be scaled up when jumping to give the illusion of a high jump.")]
    [SerializeField] private float jumpMaxScale = 2.0f;

    private Animator _animator;
    private Interactive _interactive;
    private AudioSource _audioSource;
    
    private static readonly int UnderTensionAnimatorProperty  = Animator.StringToHash("UnderTension");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _interactive = GetComponent<Interactive>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _interactive.OnInteractionTriggered += OnInteractionTriggered;
    }
    
    private void OnDisable()
    {
        _interactive.OnInteractionTriggered -= OnInteractionTriggered;
    }

    void OnInteractionTriggered(PlayerController player)
    {
        StartCoroutine(AnimateSpringJump(player));
    }

    /**
     * <summary>
     * Fully animates a jump of a player using the spring.
     * </summary>
     * <remarks>
     * The player object will undergo several adjustments during the jump, to avoid interaction with other map objects
     * during the jump.
     * <list type="bullet">
     *   <item><description><see cref="PlayerController.PhysicsEffects"/> will be disabled</description></item>
     *   <item><description>The rigidbody will be disabled.</description></item>
     *   <item><description>The layer will be changed</description></item>
     *   <item><description>...</description></item>
     * </list>
     * </remarks>
     */
    private IEnumerator AnimateSpringJump(PlayerController player)
    {
        // === 0. While in use, no other player shall use this Spring ===
        _interactive.enabled = false;
        
        // === 1. Adjust the player object so that it will not interact with other objects during the jump ===
        player.PhysicsEffects.enabled = false;
        player.PhysicsEffects.rigidbody2D.simulated = false;

        var originalLayer = player.gameObject.layer;
        player.gameObject.layer = LayerMask.NameToLayer("Carried");

        player.InteractorCollider.CanInteract = false;
        
        // Ensure the player is rendered above everything else
        var originalSortingOrder = player.spriteRenderer.sortingOrder;
        player.spriteRenderer.sortingOrder = 999;
        
        // === 2. Move player linearly to the start point. ===
        var playerTransform = player.transform;
        var currentPosition = (Vector2) playerTransform.position;
        
        while (currentPosition != (Vector2) springJumpStartPoint.transform.position)
        {
            player.transform.position = VectorExtensions.Assign2D(
                currentPosition,
                Vector2.MoveTowards(
                    currentPosition,
                    springJumpStartPoint.transform.position,
                    Time.deltaTime * jumpOnSpringSpeed
                )
            );
            
            currentPosition = playerTransform.position;
            
            // wait one frame
            yield return 0;
        }
        
        // === 3. Animate Jump ===
        
        // Animate the spring to tense up
        _animator.SetBool(UnderTensionAnimatorProperty, true);
        yield return new WaitForSeconds(underTensionTime);
        
        // Release spring and play sound
        _animator.SetBool(UnderTensionAnimatorProperty, false);
        _audioSource.Play();
        
        // Inform player object to use a jump sprite
        player.SetSpringJumpMode(true);

        // Calculate, how far the player will jump
        var jumpDistance = ((Vector2) springJumpDestination.transform.position - (Vector2) player.transform.position).magnitude;
        // We store how far we have travelled this distance here
        var traveledDistance = 0.0f;
        
        // We introduce a temporary parent object to scale the player while jumping.
        // This way, other components adjusting the player scaling can not interfere with us.
        var scalingParent = new GameObject();
        var originalParent = player.transform.parent;
        scalingParent.transform.position = player.transform.position;
        player.transform.position = Vector3.zero;
        player.transform.SetParent(scalingParent.transform, false);
        var maxScale = jumpMaxScale * Vector3.one;

        // Move the parent to the target location. This will also move the player
        currentPosition = (Vector2) scalingParent.transform.position;
        while (currentPosition != (Vector2) springJumpDestination.transform.position)
        {
            // Compute how far we can move in this frame depending on the passed time
            var distanceDelta = Time.deltaTime * jumpSpeed;
            // Accumulate the distance we moved so far. We will use this to adjust the scaling
            traveledDistance += distanceDelta;

            scalingParent.transform.position = VectorExtensions.Assign2D(
                currentPosition,
                Vector2.MoveTowards(
                    currentPosition,
                    springJumpDestination.transform.position,
                    distanceDelta
                )
            );

            currentPosition = scalingParent.transform.position;

            // Animate the scaling of the player
            // They shall appear bigger towards the middle of the travel distance, to give the illusion of a high jump
            
            // If we have not reached half the travel distance yet...
            if (traveledDistance < jumpDistance / 2)
            {
                // ...linearly interpolate the scale so that the maximum is reached at half the travel distance
                scalingParent.transform.localScale = VectorExtensions.Assign2D(
                    scalingParent.transform.localScale,
                    Vector2.Lerp(Vector2.one, maxScale, 2 * traveledDistance / jumpDistance)
                );
            }

            // Otherwise...
            else
            {
                // ...linearly interpolate the scale starting at the maximum so that the minimum is reached at the full travel distance
                scalingParent.transform.localScale = VectorExtensions.Assign2D(
                    scalingParent.transform.localScale,
                    Vector2.Lerp(maxScale, Vector2.one, 2 * traveledDistance / jumpDistance - 1)
                );
            }
            
            // wait one frame
            yield return 0;
        }
        
        // === 4. Remove all adjustments to the player object ===
        player.InteractorCollider.CanInteract = true;
        
        player.transform.SetParent(originalParent);
        player.transform.position = scalingParent.transform.position;
        Destroy(scalingParent);

        player.spriteRenderer.sortingOrder = originalSortingOrder;
        player.SetSpringJumpMode(false);
        
        player.PhysicsEffects.enabled = true;
        player.PhysicsEffects.rigidbody2D.simulated = true;

        player.gameObject.layer = originalLayer;

        // === 5. reenable interactions with this spring ===
        _interactive.enabled = true;
    }
}
