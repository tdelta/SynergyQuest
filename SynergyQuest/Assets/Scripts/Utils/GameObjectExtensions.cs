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

using System.Linq;
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
        foreach (var renderer in self.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = visible;
        }

        foreach (var collider in self.GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = visible;
        }

        foreach (var interactive in self.GetComponentsInChildren<Interactive>())
        {
            interactive.enabled = visible;
        }

        if (self.TryGetComponent(out PlayerController player))
        {
            player.cameraTracked.enabled = visible;
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
     * Tries to return an axis-aligned bounding box for this game object.
     * </summary>
     * <remarks>
     * It tries to acquire a bounding box in the following order:
     * 1. From a 2D collider
     * 2. From a renderer (sprite size)
     *
     * If all of the above fail, an empty bounding box at the position of the object is returned
     * </remarks>
     */
    public static Bounds DetermineAABB(this GameObject go)
    {
        if (go.TryGetComponent(out Collider2D collider))
        {
            return collider.bounds;
        }
        
        else if (go.TryGetComponent(out Renderer renderer))
        {
            return renderer.bounds;
        }

        else
        {
            return new Bounds(go.transform.position, Vector3.zero);
        }
    }

    public static bool IsParentOf(this GameObject self, GameObject other)
    {
        if (ReferenceEquals(self, other))
        {
            return true;
        }

        else
        {
            return Enumerable.Range(0, self.transform.childCount)
                .Select(childIdx => self.transform.GetChild(childIdx).gameObject)
                .Any(child => child.IsParentOf(other));
        }
    }
}
