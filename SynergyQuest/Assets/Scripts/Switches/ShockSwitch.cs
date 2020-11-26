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

[RequireComponent(typeof(Attackable))]
public class ShockSwitch : MonoBehaviour
{

    [SerializeField] uint timeout = 0;
    private Switch _switcher;
    private Animator _animator;
    private Attackable _attackable;

    private readonly int hitTrigger = Animator.StringToHash("Hit");
    private readonly int timeoutTrigger = Animator.StringToHash("Timeout");

    void Awake()
    {
        _switcher = GetComponent<Switch>();
        _animator = GetComponent<Animator>();
        _attackable = GetComponent<Attackable>();
    }

    private void OnEnable()
    {
        _attackable.OnAttack += OnAttack;
    }
    
    private void OnDisable()
    {
        _attackable.OnAttack -= OnAttack;
    }

    private void Activate()
    {
        _animator.SetTrigger(hitTrigger);
        _switcher.Value = true;

        if (timeout > 0)
        {
            StopCoroutine("StartTimer");
            StartCoroutine("StartTimer");
        }
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(timeout);
        _switcher.Value = false;
        _animator.SetTrigger(timeoutTrigger);
    }

    /**
     * <summary>
     * Trigger this switch if it is attacked by a non-enemy.
     * </summary>
     */
    private void OnAttack(AttackData attack)
    {
        if (!attack.attacker.CompareTag("Enemy"))
        {
            Activate();
        }
    }
}
