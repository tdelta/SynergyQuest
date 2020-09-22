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

/**
 * <summary>
 * Players have an identifying color, <see cref="PlayerColor"/> which dictates whether or not they can interact with
 * certain objects or not.
 * 
 * This component assigns an identifying color to an object. If the color is changed in the editor, other
 * color-controlled components like <see cref="ColorSwitch"/> or <see cref="ColorReplacer"/> are programmed to use the
 * same color.
 *
 * Furthermore, if this object has a <see cref="Renderer"/> which uses an material which supports
 * <see cref="PlayerColorShaderProperty"/>, the property is set to the assigned color.
 * </summary>
 */
public class Colored : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;

    public PlayerColor Color
    {
        get => color;
        set
        {
            if (color != value)
            {
                color = value;
                SetupShader();
                OnColorChanged?.Invoke(value);
            }
        }
    }

    /**
     * <summary>
     * Event which is invoked whenever the value of <see cref="Color"/> is changed
     * </summary>
     */
    public event ColorChangedAction OnColorChanged;
    public delegate void ColorChangedAction(PlayerColor newColor);

    private void Awake()
    {
        SetupShader();
    }

    private void OnValidate()
    {
        SetupShader();
        // Other components like `ColorReplacer` may depend on the color value of this component.
        // Hence we run the OnValidate methods of all other components, when this component is changed
        this.ValidateOtherComponents();
    }

    private static readonly int PlayerColorShaderProperty = Shader.PropertyToID("_PlayerColor");

    private void SetupShader()
    {
        // If this object has a renderer, and it supports the player color property, then assign the color value of this
        // component to it.
        if (
            TryGetComponent(out Renderer renderer) &&
            renderer.sharedMaterial.HasProperty(PlayerColorShaderProperty)
        )
        {
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
                
            properties.SetColor(PlayerColorShaderProperty, color.ToRGB());
                
            renderer.SetPropertyBlock(properties);
        }
    }
}
