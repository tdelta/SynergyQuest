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
 * Creates a reflection of any <c>SpriteRenderer</c> object by creating a copy flipped on the Y-axis
 * in the "WaterReflection" layer.
 * These reflections are not visible by the default camera and are instead rendered by
 * <see cref="WaterReflectionCamera"/>.
 *
 * Water ripple particles can also be spawned.
 * </summary>
 * <remarks>
 * The sprite is reflected along the axis formed by the lower y-bound of the original sprite.
 * The reflection copy's material and sprite are kept in sync with the original.
 * </remarks>
 * <seealso cref="PuddleShaderController"/>
 */
[RequireComponent(typeof(SpriteRenderer))]
public class WaterReflectable : MonoBehaviour
{
    [Tooltip("Additional offset of the reflection")]
    [SerializeField] private Vector3 reflectionOffset = new Vector3(0, 0, 0);
    [Tooltip("Additional offset for the spawning position of the water ripples.")]
    [SerializeField] private Vector3 ripplesOffset = new Vector3(0, 0, 0);
    [Tooltip("Whether water ripple particles shall be spawned or not.")]
    [SerializeField] private bool enableRipples = true;
    
    private SpriteRenderer _originalRenderer = null;
    private SpriteRenderer _rendererCopy = null;

    private MaterialPropertyBlock _materialPropertiesBuffer = null;

    private void Awake()
    {
        _originalRenderer = GetComponent<SpriteRenderer>();

        var mirrorObject = new GameObject($"Water Reflection of {name}");
        _rendererCopy = _originalRenderer.CopyComponent(mirrorObject);
        _rendererCopy.flipY = !_originalRenderer.flipY;
        
        _materialPropertiesBuffer = new MaterialPropertyBlock();
        _originalRenderer.GetPropertyBlock(_materialPropertiesBuffer);
        _rendererCopy.SetPropertyBlock(_materialPropertiesBuffer);
        
        var sprite = _originalRenderer.sprite;
        var spriteHeight = sprite.bounds.max.y - sprite.bounds.min.y;

        mirrorObject.layer = LayerMask.NameToLayer("WaterReflection");
        mirrorObject.transform.parent = this.gameObject.transform;
        mirrorObject.transform.localPosition = new Vector3(0, -spriteHeight, 0) + reflectionOffset;

        if (enableRipples)
        {
            var ripples = Instantiate(PrefabSettings.Instance.WaterRipplesPrefab, this.transform, true);
            ripples.transform.localPosition = new Vector3(0, -(sprite.pivot.y / sprite.pixelsPerUnit), 0) + ripplesOffset;
        }
    }

    private void LateUpdate()
    {
        if (_rendererCopy.sprite != _originalRenderer.sprite)
        {
            _rendererCopy.sprite = _originalRenderer.sprite;
        }

        _rendererCopy.color = _originalRenderer.color;
        _rendererCopy.flipY = !_originalRenderer.flipY;
        _rendererCopy.flipX = _originalRenderer.flipX;
        _originalRenderer.GetPropertyBlock(_materialPropertiesBuffer);
        _rendererCopy.SetPropertyBlock(_materialPropertiesBuffer);
    }
}
