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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : EnemyController {
    [SerializeField] float flightSpeed = 1;
    [SerializeField] float flightChangeTime = 1;

    float flightTimer;
    int currentMotion = 0;
    readonly int moveYProperty = Animator.StringToHash("Move Y");
    Vector2[] flightMotions = new []{new Vector2(-1, 1), new Vector2(-1, -1),
        new Vector2(1, 1), new Vector2(1, -1)};

    protected override void Start() {
        base.Start();
        flightTimer = flightChangeTime;
    }

    protected override void Update() {
        base.Update();
        flightTimer -= Time.deltaTime;

        if (flightTimer < 0) {
            currentMotion = (currentMotion + 1) % 4;
            flightTimer = flightChangeTime;
        }
    }

    protected override Vector2 ComputeOffset() {
        var flightMotion = flightMotions[currentMotion];
        var offset = Time.deltaTime * directionSpeed * direction;
        offset += flightSpeed * Time.deltaTime * flightMotion;
        Animator.SetFloat(moveYProperty, flightMotion.y);
        return offset;
    }
}
