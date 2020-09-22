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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [SerializeField] int thrust = 1;
    [SerializeField] int damageFactor = 1;

    readonly int HitTrigger = Animator.StringToHash("Hit");

    Vector2 _direction = Vector2.zero;
    Animator _animator;
    Rigidbody2D _rigidbody2D;
    PhysicsEffects _physicsEffects;
    AudioSource _audioSource;

    public BoxCollider2D Collider { get; private set; }

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
        _audioSource = GetComponent<AudioSource>();
        Collider = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        _physicsEffects.MoveBody(_rigidbody2D.position + Time.deltaTime * thrust * _direction);
    }

    /**
     * <summary>
     * Instantiates a fireball projectile at a certain spawn point and launches into a given direction.
     * It will also play sounds etc.
     * </summary>
     */
    public static FireballProjectile Launch(FireballProjectile prefab, Vector3 spawnPoint, Vector2 direction)
    {
        var instance = Instantiate(prefab, spawnPoint, Quaternion.identity);
        
        // rotate projectile into direction of flight
        instance.transform.up = -direction;
        instance._direction = direction;
        instance._audioSource.Play();

        return instance;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<EntityController>();
            player.PutDamage(damageFactor, (other.transform.position - transform.position).normalized); 
        }
        _animator.SetTrigger(HitTrigger);
    }

    void OnHitAnimationComplete()
    {
        // remove gameobject, but keep it enabled so sounds can continue playing
        transform.localScale = Vector2.zero;
        StartCoroutine(DestroyWhenReady());
    }

    IEnumerator DestroyWhenReady()
    {       
        yield return new WaitWhile(() => _audioSource.isPlaying);
        Destroy(gameObject);
    }
}
