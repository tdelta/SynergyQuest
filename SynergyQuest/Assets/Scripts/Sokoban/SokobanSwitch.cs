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

﻿using Sokoban;
using UnityEngine;

public class SokobanSwitch : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    /**
     * Sprites to use, depending on the color
     */
    [SerializeField] private SokobanSprites sprites = default;
    

    private Switch _switch;

    public delegate void SwitchChangedAction();
    public event SwitchChangedAction OnSwitchChanged;

    public void Start()
    {
        _switch = this.GetComponent<Switch>();
    }

    /**
     * Only called in editor, e.g. when changing a property
     */
    private void OnValidate()
    {
        // Change the sprite according to the assigned color
        GetComponent<SpriteRenderer>().sprite = sprites.GetSwitchSprite(this.color);
    }

    private void SetPressed(bool pressed)
    {
        _switch.Value = pressed;

        OnSwitchChanged?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_switch.Value && other.gameObject.CompareTag("Box")){
            var box = other.GetComponent<Box>();
            var pushable = other.GetComponent<Pushable>();
            
            if(pushable.state == State.Resting && box.Color.IsCompatibleWith(this.GetColor())) {
                SetPressed(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Box")){
            var box = other.gameObject.GetComponent<Box>();
            
            if(box.Color.IsCompatibleWith(this.GetColor())) {
                SetPressed(false);
            }
        }
    }
    
    public PlayerColor GetColor(){
        return color;
    }
}
