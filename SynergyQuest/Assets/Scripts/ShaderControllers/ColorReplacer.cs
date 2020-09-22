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
using UnityEngine.Serialization;

/**
 * <summary>
 * Can swap out a specific color from a sprite. E.g. parts of the sprite of player controlled platforms,
 * <see cref="PlayerControlledPlatform"/>, are colored with the color assigned to the player controlling them.
 *
 * This requires the <see cref="SpriteRenderer"/> of the object using this component to use a material which supports
 * the following properties:
 * <list type="bullet">
 *   <item><description><c>_ReplacementColor</c></description></item>
 *   <item><description><c>_ColorToReplace</c></description></item>
 * </list>
 *
 * E.g. a material using the <c>ColorReplacer</c> shader.
 *
 * Furthermore, if the game object using this component also has a <see cref="Colored"/> component, this behaviour will
 * use the RGB value of <see cref="Colored.Color"/> to override <see cref="ReplacementColor"/>.
 * </summary>
 */
[RequireComponent(typeof(SpriteRenderer))]
public class ColorReplacer : MonoBehaviour
{
    /**
     * Color which shall be replaced in sprites.
     */
    [SerializeField] private Color colorToReplace = Color.green;
    /**
     * The color with which the color shall be replaced.
     */
    [FormerlySerializedAs("_replacementColor")]
    [SerializeField]
    private Color replacementColor = Color.green;
    public Color ReplacementColor
    {
        get
        {
            return replacementColor;
        }
        set
        {
            replacementColor = value;
            PerformReplacement();
        }
    }

    /**
     * <summary>
     * Iff set, <see cref="anyOverrideColor"/> will be used instead of the RGB value of <see cref="Colored.Color"/>
     * to override <see cref="ReplacementColor"/> when this object has a <see cref="Colored"/> component and
     * <see cref="Colored.Color"/> is <see cref="PlayerColor.Any"/>.
     *
     * See also class description.
     * </summary>
     */
    [SerializeField] private bool useAnyOverrideColor = false;
    /**
     * See description of <see cref="useAnyOverrideColor"/>.
     */
    [SerializeField] private Color anyOverrideColor = PlayerColor.Any.ToRGB();
    
    private SpriteRenderer _renderer;

    private MaterialPropertyBlock _materialProperties = null;
    private static readonly int ReplacementColorProperty = Shader.PropertyToID("_ReplacementColor");
    private static readonly int ColorToReplaceProperty = Shader.PropertyToID("_ColorToReplace");
    
    void Awake()
    {
        EnsureReady();
        
        // Make sure, the renderer of this game object even supports the shader effects required by this component
        if (!_renderer.material.HasProperty(ReplacementColorProperty) ||
            !_renderer.material.HasProperty(ColorToReplaceProperty))
        {
            Debug.LogError("The material / shader of the renderer does not support the properties required by ColorReplacer.", this);
        }
        
        PerformReplacement();
    }

    private void OnValidate()
    {
        // If this object has a `Colored` component, use its color instead of the configured ReplacementColor,
        // see also class description
        if (TryGetComponent(out Colored colored))
        {
            UseColoredOverride(colored.Color);
        }
        
        PerformReplacement();
    }

    private void PerformReplacement()
    {
        EnsureReady();
        
        _renderer.GetPropertyBlock(_materialProperties);
        
        _materialProperties.SetColor(ColorToReplaceProperty, colorToReplace);
        _materialProperties.SetColor(ReplacementColorProperty, replacementColor);
        
        _renderer.SetPropertyBlock(_materialProperties);
    }

    private void EnsureReady()
    {
        if (ReferenceEquals(_materialProperties, null))
        {
            _materialProperties = new MaterialPropertyBlock();
        }

        if (ReferenceEquals(_renderer, null))
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
    }

    /**
     * <summary>
     * Adjusts the <see cref="ReplacementColor"/> to the RGB value of the given <c>playerColor</c>.
     * </summary>
     * <remarks>
     * Invoked, with the value of <see cref="Colored.Color"/> by <see cref="OnValidate"/>,
     * if this object has a <see cref="Colored"/> component.
     * </remarks>
     */
    private void UseColoredOverride(PlayerColor playerColor)
    {
        ReplacementColor = playerColor == PlayerColor.Any && useAnyOverrideColor ?
            anyOverrideColor :
            playerColor.ToRGB();
    }
}
