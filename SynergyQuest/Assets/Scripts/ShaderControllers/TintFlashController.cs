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

﻿using UnityEngine;

/**
 * This script allows to briefly color a sprite with a certain tint.
 * For example to tint an enemy red when hit.
 *
 * It must be used together with the `TintMaterial` on the sprite.
 */
public class TintFlashController : MonoBehaviour
{
    /**
     * The controller can be in three states:
     *
     * 1. No tint shall currently be applied
     * 2. A tint is applied with increasing intensity
     * 3. A tint is removed by gradually lowering its intensity
     */
    enum TintFlashState
    {
        NoTint,
        IncreasingTint,
        RemovingTint
    }
    
    /**
     * The material which is controlled.
     */
    private Material _material;
    
    /**
     * The color of the tint which shall be applied.
     * It is set by the `FlashTint` method.
     */
    private Color _tintColor;
    
    /**
     * The speed with which the intensity of the tint currently changes.
     * It is set by the `FlashTint` method and linearly interpolates the the intensity of the tint for the target
     * duration.
     */
    private float _tintSpeed; // color units / second
    
    private TintFlashState _tintState;
    
    private static readonly int TintProperty = Shader.PropertyToID("_Tint");
    
    /**
     * Applies a tint for a fixed duration. The intensity of the tint will rise to maximum until the half time of the
     * duration. Afterwards it decreases to zero.
     */
    public void FlashTint(Color tintColor, float duration)
    {
        _tintColor = tintColor;
        _tintColor.a = 0; // The alpha value controls the intensity of the tint in this shader. Start with no tint.
        
        // Compute a rate of change of tint so that maximum tint is reached halfway of the duration.
        _tintSpeed = 1 / (duration / 2);
        
        _tintState = TintFlashState.IncreasingTint;
    }

    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // If we are currently applying a tint...
        if (_tintState != TintFlashState.NoTint)
        {
            // Modify it by the current rate of change and apply it
            _tintColor.a = Mathf.Clamp01(_tintColor.a + _tintSpeed * Time.deltaTime);
            _material.SetColor(TintProperty, _tintColor);
        }
        
        switch (_tintState)
        {
            case TintFlashState.IncreasingTint:
                // If we are currently increasing the tint, stop when it reaches 1
                if (_tintColor.a >= 1)
                {
                    // When it has reached 1, start decreasing the tint again
                    _tintState = TintFlashState.RemovingTint;
                    _tintSpeed = -_tintSpeed;
                }
                break;
            
            case TintFlashState.RemovingTint:
                // If we are currently decreasing the tint, stop when it reaches 0
                if (_tintColor.a <= 0)
                {
                    _tintState = TintFlashState.NoTint;
                    _tintSpeed = 0;
                }
                break;
        }
    }
}
