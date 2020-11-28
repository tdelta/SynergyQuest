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

[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PhysicsEffects))]
[RequireComponent(typeof(Animator))]
public abstract class EntityController : MonoBehaviour {
    private Animator _animator;
    protected Animator Animator => _animator;
    
    private Rigidbody2D _rigidbody2D;
    public Rigidbody2D Rigidbody2D => _rigidbody2D;

    readonly int vulnerableTrigger = Animator.StringToHash("Vulnerable");
    readonly int hitTrigger = Animator.StringToHash("Hit");

    private PhysicsEffects _physicsEffects;
    public PhysicsEffects PhysicsEffects => _physicsEffects;

    protected Attackable attackable
    {
        get;
        private set;
    }

    protected virtual void Awake() {
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _physicsEffects = GetComponent<PhysicsEffects>();
        attackable = GetComponent<Attackable>();
    }

    private void OnEnable()
    {
        attackable.OnAttack += OnAttack;
        attackable.OnInvincibilityChanged += OnInvincibilityChanged;
    }
    
    private void OnDisable()
    {
        attackable.OnAttack -= OnAttack;
        attackable.OnInvincibilityChanged -= OnInvincibilityChanged;
    }

    protected virtual void Start() { }

    private void OnInvincibilityChanged(bool isInvincible)
    {
        if (!isInvincible)
        {
            _animator.SetTrigger(vulnerableTrigger);
        }
    }

    private void OnAttack(AttackData attack)
    {
        if (attack.Damage >= 0)
        {
            _animator.SetTrigger(hitTrigger);
        }
    }
}
