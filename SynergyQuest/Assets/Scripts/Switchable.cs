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
using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Utility class which observes the state of switches (see the `Switch` class).
 * It is <c>activated</c> iff all registered switches are active.
 * It triggers an event when the activation changes.
 * 
 * See example the <see cref="DeadManSwitch"/> for an usage example.
 * </summary>
 */
public class Switchable : MonoBehaviour
{
    /**
     * Switches which shall be observed.
     */
    [SerializeField] private Switch[] switches = new Switch[0];

    /**
     * Automatically find all switches in the scene who have a tag of the given name and bind to them
     */
    [SerializeField] private string[] autoDiscoveredSwitchTags = new string[0];

    /**
     * If this is set, the activation value of this Switchable will never return to <c>false</c> after it has been set
     * to <c>true</c> once.
     */
    [SerializeField] private bool noDeactivation = false;
    private bool _hasBeenActivatedOnce = false;
    
    /**
     * <summary>
     * Event which is invoked if the activation value changes, see class description.
     * It is *not* invoked when computing the activation value for the first time in the <c>OnEnable</c> phase.
     * If you need the activation value when starting a scene, look up <see cref="Activation"/> in the <c>Start</c>
     * method of your component.
     * </summary>
     */
    public event ActivationChangedAction OnActivationChanged;
    public delegate void ActivationChangedAction(bool activation);

    /**
     * <summary>
     * Stores, whether all observed switches are currently active
     * </summary>
     */
    public bool Activation => _activation;
    private bool _activation = true;
    
    /**
     * Caches the current value of every switch
     */
    private bool[] _switchValues;
    /**
     * Caches the event handlers we registered on every switch, so that we can unregister them in `OnDisable`.
     */
    private Switch.ValueChangedAction[] _switchChangeHandlers;

    void Awake()
    {
        AcquireSwitches();
    }

    /**
     * <summary>
     * Discovers switches activating this component and allocates handlers for changes of their values.
     * </summary>
     */
    private void AcquireSwitches()
    {
        // Auto-discover switches by tags and add them to the manually configured ones
        switches = autoDiscoveredSwitchTags
            .SelectMany(GameObject.FindGameObjectsWithTag)
            .Select(gameObject => gameObject.GetComponent<Switch>())
            .Concat(switches)
            .Distinct() // Have no duplicate switches
            .ToArray();

        // Allocate memory in our caches for every switch
        _switchValues = new bool[switches.Length];
        _switchChangeHandlers = new Switch.ValueChangedAction[switches.Length];
    }

    public void AddSwitches(params Switch[] additionalSwitches)
    {
        OnDisable();
        switches = switches.Concat(additionalSwitches).ToArray();
        AcquireSwitches();
        OnEnable();
    }

    private void OnEnable()
    {
        // For every switch which shall be observed, register an event handler
        for (int i = 0; i < switches.Length; ++i)
        {
            var switchObj = switches[i];
            if ( switchObj != null )
            {
                _switchValues[i] = false;
                
                var localI = i;
                _switchChangeHandlers[i] = value =>
                {
                    OnSwitchValueChanged(localI, value);
                };

                switchObj.OnValueChanged += _switchChangeHandlers[i];
            }
        }
        
        ComputeActivation(suppressCallbacks: true);
    }

    private void OnDisable()
    {
        // For every switch which shall is observed, unregister our event handler
        for (int i = 0; i < switches.Length; ++i)
        {
            var switchObj = switches[i];
            if (i < _switchChangeHandlers.Length && switchObj != null)
            {
                switchObj.OnValueChanged -= _switchChangeHandlers[i];
            }
        }
    }

    /**
     * <summary>
     * Determine, whether all observed switches are currently activated at once
     * </summary>
     * <param name="suppressCallbacks">Iff true, no subscribers of the `OnActivationChanged` event will be informed when the activation changes. Default: false</param>
     */
    void ComputeActivation(bool suppressCallbacks = false)
    {
        var oldActivation = _activation;

        // If the <c>noDeactivation</c> property is set to true, we no longer change the activation if it has been set to
        // <c>true</c> once. Otherwise, we update the activation based on the values of the registered switches.
        if (!noDeactivation || !_hasBeenActivatedOnce)
        {
            _activation = _switchValues.All(v => v);
            _hasBeenActivatedOnce = _activation || _hasBeenActivatedOnce;
        }

        // If the activation state of this component changed, inform all subscribers
        if (!suppressCallbacks && oldActivation != _activation)
        {
            OnActivationChanged?.Invoke(_activation);
        }
    }

    /**
     * Called, if an observed switch changes its value.
     */
    void OnSwitchValueChanged(int switchIdx, bool value)
    {
        // Store the new switch value
        _switchValues[switchIdx] = value;
        // Determine, if now all switches are pressed.
        ComputeActivation();
    }
}
