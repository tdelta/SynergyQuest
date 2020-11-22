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

public class BossSpawn : MonoBehaviour
{
    [SerializeField] EnemyController boss = default;
    [SerializeField] AudioClip bossInactive = default;
    [SerializeField] AudioClip bossActive = default;

    Switchable _switchable;
    Switch _switch;
    AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _switchable = GetComponent<Switchable>();
        _switch = GetComponent<Switch>();
    }

    void Start()
    {
        if (!_switch.Value)
            _switchable.OnActivationChanged += OnSwitchActivate;
    }

    void OnSwitchActivate(bool activated)
    {
        if (activated)
        {
            _switchable.OnActivationChanged -= OnSwitchActivate;
            var instance = Instantiate(boss, transform.position, Quaternion.identity);
            instance.GetComponent<Health>().OnDeath += OnBossDead;
            instance.ShowParticles();
            _audioSource.clip = bossActive;
            _audioSource.Play();
        }
    }

    void OnBossDead()
    {
        _switch.Value = true;
        _audioSource.clip = bossInactive;
        _audioSource.Play();
    }
}
