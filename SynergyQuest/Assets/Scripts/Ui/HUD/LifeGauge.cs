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

 using System.Collections.Generic;
using UnityEngine;

/**
 * Draws a bar of hearts which symbolizes a characters remaining HP. It slowly fades aways.
 */
public class LifeGauge : MonoBehaviour
{
    /**
     * Current transparency. It is animated to induce a fade-out.
     */
    [SerializeField] private float alphaValue = 1.0f;
    
    /**
     * Background of the gauge
     */
    [SerializeField] private SpriteRenderer lifeGaugeBg = default;
    
    /**
     * Sources of heart sprites. Empty hearts are used to draw lost lives.
     */
    [SerializeField] private SpriteRenderer heartObjectPrefab = default;
    [SerializeField] private SpriteRenderer emptyHeartObjectPrefab = default;

    class SpritePool : ObjectPool<SpriteRenderer> { };
    private SpritePool _heartObjectPool = default;
    private SpritePool _emptyHeartObjectPool = default;

    private Animator _animator;

    /**
     * We cache the dimensions of a heart here, since they are needed to dynamically render the gauge.
     */
    private Vector2 _heartSize;

    /**
     * We cache active instances of hearts here, so that we can manipulate their transparency values.
     */
    private List<SpriteRenderer> _heartInstances = new List<SpriteRenderer>();
    private List<SpriteRenderer> _emptyHeartInstances = new List<SpriteRenderer>();
    
    /**
     * Animation trigger for the fade-out effect.
     */
    private static readonly int FadeOutTrigger = Animator.StringToHash("FadeOut");

    void Awake()
    {
        _animator = GetComponent<Animator>();
        
        _heartObjectPool = ObjectPool.Make<SpritePool, SpriteRenderer>(this.transform, heartObjectPrefab);
        _emptyHeartObjectPool = ObjectPool.Make<SpritePool, SpriteRenderer>(this.transform, emptyHeartObjectPrefab);
        
        _heartSize = _heartObjectPool.Prefab.bounds.size;
    }

    /**
     * Runs everytime the animation changes the alpha value. It forwards the value to all sprites in the gauge.
     */
    private void OnDidApplyAnimationProperties()
    {
        SetAlpha(lifeGaugeBg);
        _heartInstances.ForEach(SetAlpha);
        _emptyHeartInstances.ForEach(SetAlpha);
    }

    /**
     * Sets the alpha value of a sprite to `this.alphaValue`
     */
    private void SetAlpha(SpriteRenderer renderer)
    {
        var oldColor = renderer.material.color;
        var newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaValue);
        renderer.GetComponent<SpriteRenderer>().material.color = newColor;
    }

    private void OnEnable()
    {
        // As soon as the gauge is enabled, it begins to fade out
        TriggerFadeOut();
    }
    
    /**
     * Called by the animation behavior (`FadedOutBehavior.cs`), as soon as the gauge is fully faded out.
     */
    public void OnFadedOut()
    {
        this.gameObject.SetActive(false);
    }

    /**
     * Draws the life gauge
     *
     * @param health    current health of a character
     * @param maxHealth maximum health of that character (so that lost lives can be displayed)
     */
    public void DrawLifeGauge(int health, int maxHealth)
    {
        this.gameObject.SetActive(true);
        // Clear out previous layout of the gauge.
        ClearHeartInstances();
        
        // How far apart shall hearts be?
        var heartGapWidth = _heartSize.x * 0.3f; // -> 30% of their width
        
        // How wide will the complete row of hearts be?
        // (It's computed from the width of a single heart, the maximum number of HP and the gaps between every heart)
        var heartBarWidth = _heartSize.x * maxHealth + heartGapWidth * (maxHealth - 1);

        // How wide shall the background of the gauge be?
        var bgWidth = heartBarWidth + _heartSize.x * 0.3f * 2; // <- As long as the row of hearts + 30% of the width of a heart on each side
        // How high shall the background be?
        var bgHeight = _heartSize.y * 1.3f; // <- as high as a heart + 30% of a heart length.
        var bgSize = new Vector2(bgWidth, bgHeight);

        // Adapt the size of the background
        lifeGaugeBg.size = bgSize;
        
        // Where shall the first heart be drawn?
        var heartDrawPosition = new Vector3(
            -heartBarWidth / 2 + 0.5f * _heartSize.x,
            // ^ we move the start position to the left from the origin by half the width of the complete bar. Since the heart sprites are centered around their position, we also must substract half a heart width
            0.0f
        );
        
        // By what amount shifts this position for every subsequent heart?
        var heartDrawPositionDelta = new Vector3(
            _heartSize.x + heartGapWidth, // <- by the width of a heart + the gap between hearts
            0,
            0
        );

        // Draw a heart for every (lost) life:
        for (int i = 0; i < maxHealth; ++i)
        {
            SpriteRenderer heart;
            if (i < health) // If the counter is less that the current health, we draw full hearts
            {
                heart = _heartObjectPool.GetInstance(lifeGaugeBg.transform);
                _heartInstances.Add(heart);
            }

            else // otherwise, the health point has been lost and we draw an empty heart
            {
                heart = _emptyHeartObjectPool.GetInstance(lifeGaugeBg.transform);
                _emptyHeartInstances.Add(heart);
            }

            // position the heart instance
            heart.transform.localPosition = heartDrawPosition;

            // Compute the position where we draw the next heart:
            heartDrawPosition += heartDrawPositionDelta;
        }
        
        // Start the fade-out animation
        TriggerFadeOut();
    }

    /**
     * Starts the fade-out animation
     */
    private void TriggerFadeOut()
    {
        _animator.SetTrigger(FadeOutTrigger);
    }
    
    /**
     * Returns all heart instances to the object pools.
     */
    private void ClearHeartInstances()
    {
        _heartInstances.ForEach(heart => _heartObjectPool.ReturnInstance(heart));
        _heartInstances.Clear();
        
        _emptyHeartInstances.ForEach(heart => _emptyHeartObjectPool.ReturnInstance(heart));
        _emptyHeartInstances.Clear();
    }
}
