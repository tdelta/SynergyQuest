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

using Cinemachine;
using UnityEngine;

public static class GameObjectExtensions
{
    /**
     * Ensures a game object does / does not interact with other game objects.
     * 
     * Unlike SetActive, it does not disable all functionality of an object.
     * Instead, currently it disables any renderer and 2d collider.
     *
     * @param visible true iff the object shall interact / be visible
     */
    public static void SetVisibility(this GameObject self, bool visible)
    {
        if (self.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.enabled = visible;
        }

        if (self.TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = visible;
        }

        foreach (var interactive in self.GetComponents<Interactive>())
        {
            interactive.enabled = visible;
        }

        if (self.CompareTag("Player"))
        {
            self.SetFollowedByCamera(visible);
        }
    }

    public static void MakeInvisible(this GameObject self)
    {
        self.SetVisibility(false);
    }

    public static void MakeVisible(this GameObject self)
    {
        self.SetVisibility(true);
    }

    /**
     * <summary>
     * Ensures that a physics controlled entity can no longer move due to physics.
     * Can also unfreeze by setting the parameter to <c>false</c>
     * </summary>
     * <seealso cref="UnFreeze"/>
     */
    public static void Freeze(this GameObject self, bool freeze = true)
    {
        if (self.GetComponent<Rigidbody2D>() is Rigidbody2D body && body != null)
        {
            body.simulated = !freeze;
        }
    }
    
    /**
     * <seealso cref="Freeze"/>
     */
    public static void UnFreeze(this GameObject self)
    {
        self.Freeze(false);
    }
    
    /**
     * Destroys a game object after playing a sound once.
     * Until the sound finishes playing and the game object is destroyed, it is modified to be invisible and to not
     * interact with the scene.
     *
     * Requires that the game object has an `AudioSource`.
     */
    public static void PlaySoundAndDestroy(this GameObject self)
    {
        self.MakeInvisible();

        var audioSource = self.GetComponent<AudioSource>();
        var waitTimeUntilDestroy = audioSource?.clip?.length ?? 0;
        
        audioSource.Play();
        
        Object.Destroy(self, waitTimeUntilDestroy);
    }
    
    // FIXME: Implement as proper animation
    public static void Shrink(this GameObject self, Vector3 scaleFactor)
    {
        if (self.GetComponent<PhysicsEffects>() is PhysicsEffects effects)
        {
            if (effects.GetImpulse() == Vector2.zero)
            {
                self.transform.localScale -= scaleFactor;
            }
        }
    }

    /**
     * <summary>
     * Can set all <see cref="CinemachineTargetGroup"/>s to either follow or not follow an object.
     * </summary>
     * <param name="followed">Whether the object shall be followed by cameras.</param>
     * <param name="radius">
     *     Radius around every object which must be included in the camera view.
     *     If set to NaN (default), the bounds of a <see cref="Collider2D"/> component will be used, if present.
     *     If there is also no <see cref="Collider2D"/>, then 0.0f will be used.
     * </param>
     */
    public static void SetFollowedByCamera(this GameObject self, bool followed, float radius = float.NaN)
    {
        if (followed)
        {
            foreach (var cameraTargetGroup in Object.FindObjectsOfType<CinemachineTargetGroup>())
            {
                if (float.IsNaN(radius))
                {
                    radius = 0.0f;
                    if (self.TryGetComponent(out PlayerController _))
                    {
                        radius = CameraSettings.Instance.PlayerInclusionRadius;
                    }
                    
                    else if (self.TryGetComponent(out Collider2D collider))
                    {
                        var bounds = collider.bounds;
                        radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
                    }
                }
                
                cameraTargetGroup.AddMember(
                    self.transform,
                    1,
                    radius
                );
            }
        }

        else
        {
            foreach (var cameraTargetGroup in Object.FindObjectsOfType<CinemachineTargetGroup>())
            {
                cameraTargetGroup.RemoveMember(self.transform);
            }
        }
    }
}
