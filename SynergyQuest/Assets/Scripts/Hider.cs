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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Switchable))]
public class Hider : MonoBehaviour
{
    // List of objects which will be made invisible / visible
    [SerializeField] private List<GameObject> objects = default;
    
    private Switchable _switchable;

    // Start is called before the first frame update
    void Awake()
    {
        _switchable = this.GetComponent<Switchable>();    
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
        // Apply the initial activation to all managed objects
        foreach (var o in objects.Where(o => o != null))
        {
            o.SetVisibility(_switchable.Activation);
        }
    }

    private void OnActivationChanged(bool activation)
    {
        foreach (var o in objects.Where(o => o != null))
        {
            if (Teleport.SupportsTeleportEffect(o))
            {
                if (activation)
                {
                    Teleport.TeleportIn(o, Color.cyan);
                }

                else
                {
                    Teleport.TeleportOut(o, Color.cyan);
                }
            }

            else
            {
                o.SetVisibility(activation);
            }
        }
    }
}
