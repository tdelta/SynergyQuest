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
 * Allows to change the clip played by a audio source using one or more switches.
 *
 * For example, this behaviour can be used to change the background music of a monster room when all monsters have been
 * defeated.
 */
[RequireComponent(typeof(AudioSource), typeof(Switchable))]
public class SwitchableAudio : MonoBehaviour
{
    [SerializeField] private AudioClip initialAudio = default;
    [SerializeField] private AudioClip switchedAudio = default;
    
    private AudioSource _audioSource;
    private Switchable _switchable;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _switchable = GetComponent<Switchable>();
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }

    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Start()
    {
        OnActivationChanged(_switchable.Activation);
    }

    private void OnActivationChanged(bool activation)
    {
        _audioSource.clip = activation ? switchedAudio : initialAudio;
        _audioSource.Play();
    }
}
