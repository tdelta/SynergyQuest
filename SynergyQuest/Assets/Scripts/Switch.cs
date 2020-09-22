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

﻿using System;
using UnityEngine;

/**
 * Utility component which represents a switch which can be activated or deactivated by players.
 * It only provides the logic to propagate the state of the switch to other components which use the `Switchable`
 * component:
 *
 * The change is propagated by providing an event `OnValueChange`, which other objects can subscribe to.
 *
 * The logic which triggers a change of the state of the switch must be provided by another component, see for example
 * the `DeadManSwitch` class.
 * The change can be triggered by setting the `Value` property.
 */
public class Switch : MonoBehaviour
{
    /**
     * Current value of the switch (activated / not activated)
     */
    [SerializeField] private bool value = false;
    /**
     * If set, this switch will remember its value when switching scenes at runtime.
     * Use this property to keep doors opened, if a puzzle has already been solved
     */
    [SerializeField] private bool isPersistentAcrossScenes = false;

    /**
     * Event which is triggered when the state of this switch changes
     */
    public event ValueChangedAction OnValueChanged;
    public delegate void ValueChangedAction(bool value);

    public bool Value
    {
        get => value;
        set
        {
            var oldValue = this.value;
            this.value = value;

            // Only trigger the `OnValueChange` event, if the value actually changed.
            // Hence it does not trigger if the same value is set as before.
            if (oldValue != this.value)
            {
                if (isPersistentAcrossScenes)
                {
                    DungeonDataKeeper.Instance.SaveSwitchActivation(this);
                }
                OnValueChanged?.Invoke(value);
            }
        }
    }

    public Guid Guid { get; private set; }

    private void Awake()
    {
        Guid = GetComponent<Guid>();
        isPersistentAcrossScenes = !(Guid is null) && isPersistentAcrossScenes;
    }

    private void Start()
    {
        if (isPersistentAcrossScenes)
        {
            Value = DungeonDataKeeper.Instance.HasSwitchBeenActivated(this, Value);
        }
    }

    private void OnValidate()
    {
        if (isPersistentAcrossScenes && GetComponent<Guid>() is null)
        {
            isPersistentAcrossScenes = false;
            Debug.LogError("Can not make switch persistent, if there is no Guid component.");
        }
    }
}
