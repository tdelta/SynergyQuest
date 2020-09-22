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
 * Adjusts the transform of an object so that it spans across the horizontal distance between two other objects.
 * It also runs in editor mode, so that the adjustments performed by this behaviour are visible while editing a scene.
 *
 * An example use case of this behaviour are the light barriers used in in platform levels. This behaviour is applied,
 * so that the differently colored light barriers completely fill the space between each other, even when some part of
 * the barrier moves.
 * </summary>
 */
[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class SizeLimited : MonoBehaviour
{
    /**
     * If this property is set, this objects' transform is adjusted, so that its left right boundary begins at the
     * position of the given transform.
     *
     * If the object of the transform also has a collider, the limiter position is extended by the bounds of the
     * collider.
     */
    [SerializeField] private Transform rightLimiter = default;
    /**
     * If this property is set, this objects' transform is adjusted, so that its left boundary begins at the position
     * of the given transform.
     * 
     * If the object of the transform also has a collider, the limiter position is extended by the bounds of the
     * collider.
     */
    [SerializeField] private Transform leftLimiter = default;
    
    /**
     * Size of this object when not stretched/resized
     */
    [SerializeField] private Vector2 originalSize = Vector2.one;

    /**
     * Cache values set during the <c>Start</c>-phase.
     * They indicate, which of the above transforms have been set. We use these booleans, as checking them is more
     * efficient then frequently comparing the above Transform references to null.
     * 
     * (this is because Unity overrides the == operator for null values, see also
     * https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/)
     */
    private bool _hasRightLimiter = false;
    private bool _hasLeftLimiter = false;

    private float _rightOffset = 0;
    private float _leftOffset = 0;

    private float _cachedRight = float.NaN;
    private float _cachedLeft = float.NaN;
    
    private Vector3 _scaleBuffer = Vector3.zero;

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnValidate()
    {
        Awake();
        Start();
        Update();
    }

    private void Start()
    {
        _hasRightLimiter = rightLimiter != null;
        _hasLeftLimiter = leftLimiter != null;

        _scaleBuffer = this.transform.localScale;
    }

    private void Update()
    {
        #if UNITY_EDITOR
            // When editing, always reset the cached values, since it is not guaranteed, that this object is
            // reinitialized if something in the scene changes and Update is rerun
            _cachedLeft = float.NaN;
            _cachedRight = float.NaN;
        #endif
        
        UpdateSizeAndPosition();
    }

    void UpdateSizeAndPosition()
    {
        var needsUpdate = false;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_hasRightLimiter && rightLimiter.position.x != _cachedRight)
        {
            _cachedRight = rightLimiter.position.x;
            if (rightLimiter.TryGetComponent(out BoxCollider2D rightCollider))
            {
                _rightOffset = -rightCollider.bounds.extents.x;
            }

            else
            {
                _rightOffset = 0;
            }
            
            needsUpdate = true;
        }
        
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_hasLeftLimiter && leftLimiter.position.x != _cachedLeft)
        {
            _cachedLeft = leftLimiter.position.x;
            if (leftLimiter.TryGetComponent(out Collider2D leftCollider))
            {
                _leftOffset = leftCollider.bounds.extents.x;
            }
            
            else
            {
                _leftOffset = 0;
            }
            
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            var position = this.transform.position;

            var rightLimit = position.x + originalSize.x / 2;
            if (_hasRightLimiter)
            {
                rightLimit = _cachedRight + _rightOffset;
            }

            var leftLimit = rightLimit - originalSize.x;
            if (_hasLeftLimiter)
            {
                leftLimit = _cachedLeft + _leftOffset;
            }

            position.x = 0.5f * (leftLimit + rightLimit);
            _scaleBuffer.x = (rightLimit - leftLimit) / originalSize.x;
            
            var transformRef = transform;
            transformRef.position = position;
            transformRef.localScale = _scaleBuffer;
        }
    }
}
