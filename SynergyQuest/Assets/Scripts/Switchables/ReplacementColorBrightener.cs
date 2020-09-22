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
 * The <see cref="ColorReplacer"/> component allows to replace colors in sprites. This component allows to temporarily
 * brighten this color.
 * This is for example useful to show that a colored switch activates, e.g. <see cref="ColorSwitch"/>.
 * </summary>
 */
[RequireComponent(typeof(Switchable), typeof(ColorReplacer))]
public class ReplacementColorBrightener : MonoBehaviour
{
    /**
     * How much shall this component temporarily brighten the replacement color?
     */
    [SerializeField] private float brightnessDelta = 0.2f;
    
    private Switchable _switchable;
    private ColorReplacer _colorReplacer;

    // We cache the colors in these fields
    private Color _beforeActivationColor; // Color used by the ColorReplacer before applying this effect

    private void Awake()
    {
        _colorReplacer = GetComponent<ColorReplacer>();
        _switchable = GetComponent<Switchable>();
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
        _beforeActivationColor = _colorReplacer.ReplacementColor;
        
        // The Switchable component does not trigger this callback by itself for the initial activation when loading the
        // scene. Hence, we look the initial value up ourselves
        OnActivationChanged(_switchable.Activation);
    }

    void OnActivationChanged(bool activation)
    {
        if (activation)
        {
            _beforeActivationColor = _colorReplacer.ReplacementColor;
            
            _colorReplacer.ReplacementColor = new Color(
                Mathf.Min(1.0f, _beforeActivationColor.r + brightnessDelta),
                Mathf.Min(1.0f, _beforeActivationColor.g + brightnessDelta),
                Mathf.Min(1.0f, _beforeActivationColor.b + brightnessDelta),
                _beforeActivationColor.a
            );
        }

        else
        {
            _colorReplacer.ReplacementColor = _beforeActivationColor;
        }
    }
}
