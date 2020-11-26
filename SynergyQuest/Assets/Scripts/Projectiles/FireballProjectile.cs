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
using DamageSystem;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

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

    /**
     * <summary>
     * Source of this fireball. E.g. the monster which created it.
     * </summary>
     */
    private GameObject _source;

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
     * <param name="source">The game object which fired this projectile.</param>
     */
    public static FireballProjectile Launch(GameObject source, FireballProjectile prefab, Vector3 spawnPoint, Vector2 direction)
    {
        var instance = Instantiate(prefab, spawnPoint, Quaternion.identity);

        instance._source = source;
        // rotate projectile into direction of flight
        instance.transform.up = -direction;
        instance._direction = direction;
        instance._audioSource.Play();

        return instance;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject != _source && other.gameObject.TryGetComponent(out Attackable attackable))
        {
            attackable.Attack(new AttackData
            {
                attacker = _source,
                damage = damageFactor,
                knockback = 4,
                attackDirection = Optional.Some<Vector2>((other.transform.position - transform.position).normalized)
            });
        }
        _animator.SetTrigger(HitTrigger);
    }

    void OnHitAnimationComplete()
    {
        // remove gameobject, but keep it enabled so sounds can continue playing
        transform.localScale = Vector2.zero;
        StartCoroutine(DestroyWhenReady());
        GetComponentInChildren<Light2D>().enabled = false;
    }

    IEnumerator DestroyWhenReady()
    {       
        yield return new WaitWhile(() => _audioSource.isPlaying);
        Destroy(gameObject);
    }
}
