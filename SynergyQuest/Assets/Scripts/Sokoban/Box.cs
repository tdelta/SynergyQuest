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
using System.Runtime.InteropServices;
using Sokoban;
using UnityEngine;

/**
 * Assigns a sokoban box the correct sprite, depending on the color of its `Pushable` component.
 */
[RequireComponent(typeof(Interactive))]
public class Box : MonoBehaviour
{
    /**
     * Sprites to use, depending on the color
     */
    [SerializeField] private SokobanSprites sprites = default;

    private Interactive _interactive;
    public PlayerColor Color => _interactive.Color;

    private void Awake()
    {
        _interactive = GetComponent<Interactive>();
    }

    /**
     * Only called in editor, e.g. when changing a property
     */
    public void OnValidate()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var interactive = GetComponent<Interactive>();

        // Change the sprite according to the assigned color
        spriteRenderer.sprite = sprites.GetBoxSprite(interactive.Color);
    }
}
