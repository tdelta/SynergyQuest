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
 * <summary>
 * An entity which periodically fires projectiles into a certain direction.
 * </summary>
 */
public class Turret : MonoBehaviour
{
    /**
     * How often a projectile shall be fired (every n seconds)
     */
    [SerializeField] private float fireRate = 2.0f;
    /**
     * Where the projectile shall be fired from
     */
    [SerializeField] private Transform launchPoint = null;
    /**
     * In what direction the projectile shall be fired
     */
    [SerializeField] private Vector2 direction = Vector2.zero;
    /**
     * The projectile to fire
     */
    [SerializeField] FireballProjectile fireballPrefab = default;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Call the given method repeatedly to launch a projectile 
        InvokeRepeating(nameof(LaunchProjectile), 0.0f, fireRate);
    }

    void LaunchProjectile()
    {
        var instance = FireballProjectile.Launch(this.gameObject, fireballPrefab, this.launchPoint.position, direction);

        // If this object has a collider, the projectile shall not collide with it
        if (!ReferenceEquals(_collider, null))
        {
            Physics2D.IgnoreCollision(instance.Collider, _collider);
        }
    }
}
