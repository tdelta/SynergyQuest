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

namespace DamageSystem
{
    /**
     * <summary>
     * Models "health points" of an object.
     * </summary>
     * <remarks>
     * * it requires an <see cref="Attackable"/> behavior to be present and adjusts the health points according to incoming attacks.
     * * other objects may listen to the events of this behavior to react to changes to the health points
     * * since players and monsters manage health in different ways, the actual backend where the health value is saved
     *   may be swapped out. See <see cref="SetCustomSaver"/> and <see cref="IHealthSaver"/>.
     * </remarks>
     */
    [RequireComponent(typeof(Attackable))]
    public class Health : MonoBehaviour
    {
        /**
         * Backend to store the current health points value.
         */
        private IHealthSaver _healthSaver = new DefaultHealthSaver();

        /**
         * Use a custom backend to store the health value.
         * Can for example be called to use a persistent storage, e.g. players use <see cref="PlayerData"/> so that their
         * health points are persistently remembered across scenes.
         */
        public void SetCustomSaver(IHealthSaver healthSaver)
        {
            _healthSaver = healthSaver;
            _healthSaver.InitHealthPoints(MaxValue);
        }
        
        [SerializeField, Tooltip("Maximum health points")] private int maxValue = 1;
        public int MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                if (Value > maxValue)
                {
                    Value = maxValue;
                }
            }
        }

        /**
         * <summary>
         * How much health points the owner of this component has.
         * </summary>
         */
        public int Value
        {
            get => _healthSaver.HealthPoints;
            private set
            {
                // Only do something, if the new value is actually different from the old one
                if (value != _healthSaver.HealthPoints)
                {
                    _healthSaver.HealthPoints = value;
                    if (value > maxValue)
                    {
                        Value = maxValue;
                    }
                    
                    OnHealthChanged?.Invoke(value);
                    if (value <= 0)
                    {
                        OnDeath?.Invoke();
                    }
                }
            }
        }
        
        public bool IsDead => Value <= 0;

        /**
         * <summary>
         * Emitted whenever the health points change.
         * </summary>
         */
        public event HealthChangedAction OnHealthChanged;
        public delegate void HealthChangedAction(int value);
        
        /**
         * <summary>
         * Emitted when the health points reach 0.
         * </summary>
         */
        public event DeathAction OnDeath;
        public delegate void DeathAction();

        private Attackable _attackable;

        private void Awake()
        {
            _healthSaver.InitHealthPoints(maxValue);
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

        private void OnAttack(AttackData attack)
        {
            // ReSharper disable once Unity.NoNullPropagation
            if (DebugSettings.Instance.Instagib && attack.Damage > 0 && (attack.Attacker?.CompareTag("Player") ?? false))
            {
                // Kill instantly, if instagib debug option is activated and the attacker is a player
                Value = 0;
            }

            else
            {
                // Otherwise subtract damage normally
                Value -= attack.Damage;
            }
        }

        public void Reset()
        {
            Value = MaxValue;
        }
    }
}