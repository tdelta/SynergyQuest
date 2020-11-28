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

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Effects
{
    /**
     * <summary>
     * Animates blood particles in direction of an attack
     * </summary>
     */
    [RequireComponent(typeof(ParticleSystem))]
    public class BloodParticles : MonoBehaviour
    {
        /**
         * <summary>
         * Highest color value a blood particle may have.
         * The color of the blood particles is randomly chosen between <see cref="lightColor"/> and
         * <see cref="darkColor"/>.
         * </summary>
         */
        public Color lightColor = new Color(1, 0.149f, 0, 1);
        /**
         * <summary>
         * Lowest color value a blood particle may have.
         * The color of the blood particles is randomly chosen between <see cref="lightColor"/> and
         * <see cref="darkColor"/>.
         * </summary>
         */
        public Color darkColor = new Color(0.733f, 0.0784f, 00, 1);
        
        /**
         * <summary>
         * Callback which is invoked when the underlying particle system finishes animating.
         * You can set it to delete this object after the animation finishes.
         * </summary>
         */
        [CanBeNull, NonSerialized] public DoneAction DoneCallback = default;
        public delegate void DoneAction();
            
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            var main = _particleSystem.main;
            var startColor = main.startColor;
            startColor.colorMin = lightColor;
            startColor.colorMax = darkColor;

            main.startColor = startColor;

            main.stopAction = ParticleSystemStopAction.Callback;
        }

        /**
         * <summary>
         * Start the animation. The blood particles will fly in the given direction.
         * </summary>
         */
        public void Trigger(Vector2 direction)
        {
            var rotation = Quaternion.FromToRotation(
                Vector2.up,
                direction
            );
            _particleSystem.transform.rotation = rotation;
            
            _particleSystem.Play();
        }

        private void OnParticleSystemStopped()
        {
            DoneCallback?.Invoke();
        }
    }
}