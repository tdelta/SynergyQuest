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

using DamageSystem;
using UnityEngine;

public class Bomb : Item
{
    [SerializeField] ParticleSystem sparkEffect = default;

    bool explosion = false;
    readonly int explosionTrigger = Animator.StringToHash("Explode");

    private Animator _animator;
    private PhysicsEffects _effects;
    private Rigidbody2D _rigidbody2D;
    private AudioSource _audioSource;
    private Throwable _throwable;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _effects = GetComponent<PhysicsEffects>();
        _audioSource = GetComponent<AudioSource>();
        _throwable = GetComponent<Throwable>();
    }

    private void Start()
    {
        _animator.SetTrigger(explosionTrigger);
    }
    
    protected override void OnActivate(PlayerController player)
    {
        _throwable.Pickup(player);
    }

    /**
     * Invoked by the animation controller when the bomb shall start exploding
     */
    public void Explode()
    {
        _audioSource.Play();
        sparkEffect.Stop();
        sparkEffect.GetComponent<AudioSource>().Stop();
        explosion = true;

        // Checks whether the bomb is still carried when it explodes. If so, damage the carrier
        if (_throwable.IsBeingCarried) {
            if (_throwable.Carrier.TryGetComponent(out Attackable carrierAttackable))
            {
                carrierAttackable.Attack(new WritableAttackData
                {
                    Attacker = gameObject,
                    Damage = 1,
                    Knockback = 4,
                    AttackDirection = Optional.Some<Vector2>((_throwable.Carrier.transform.position - transform.position).normalized)
                });
            }
        }

    }

    /**
     * Invoked by animation after bomb exploded
     */
    public void Destroy()
    {
        Destroy(gameObject);
    }

    void FixedUpdate()
    {
        if (!explosion)
            _effects.MoveBody(_rigidbody2D.position);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (explosion) 
        {
            var otherGameobject = other.collider.gameObject;
            if (otherGameobject.TryGetComponent(out Attackable otherAttackable))
            {
                otherAttackable.Attack(new WritableAttackData
                {
                    Attacker = gameObject,
                    Damage = 1,
                    Knockback = 4,
                    AttackDirection = Optional.Some<Vector2>((other.transform.position - transform.position).normalized)
                });
            }
            else if (otherGameobject.CompareTag("DestroyableWall"))
                Destroy(otherGameobject);
        }
    }
    
}
