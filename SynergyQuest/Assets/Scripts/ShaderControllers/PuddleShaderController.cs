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
 * Retrieves and sets the render texture which contains object reflections for the <c>Puddle</c> shader.
 * </summary>
 * <remarks>
 * The render texture is created and rendered by <see cref="WaterReflectionCamera"/>.
 * A reflection of a sprite is created and rendered, if the object has the <see cref="WaterReflectable"/> behaviour.
 * </remarks>
 */
public class PuddleShaderController : MonoBehaviour
{
    private Renderer _renderer;
    private WaterReflectionCamera _waterReflectionCamera;

    private MaterialPropertyBlock _materialPropertiesBuffer = null;
    
    private static readonly int ReflectedRenderTextureProperty = Shader.PropertyToID("_ReflectedRenderTexture");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _waterReflectionCamera = WaterReflectionCamera.Instance;
        _materialPropertiesBuffer = new MaterialPropertyBlock();
    }
    
    private void OnEnable()
    {
        _waterReflectionCamera.OnRenderTextureUpdated += OnRenderTextureUpdated;
    }
    
    private void OnDisable()
    {
        _waterReflectionCamera.OnRenderTextureUpdated -= OnRenderTextureUpdated;
    }

    private void OnRenderTextureUpdated()
    {
        _renderer.GetPropertyBlock(_materialPropertiesBuffer);
        _materialPropertiesBuffer.SetTexture(ReflectedRenderTextureProperty, _waterReflectionCamera.RenderTexture);
        _renderer.SetPropertyBlock(_materialPropertiesBuffer);
    }
}
