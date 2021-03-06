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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * Manages a small speech bubble displayed on top of players.
 * After being invoked, it fades after a short amount of time.
 *
 * It is intended to give the player hints about possible interactions with an object by displaying the name of the
 * button they can press.
 * For every type of interaction button, a speech bubble text and color can be defined, see the static field
 * `colorSettings`.
 *
 * The Bubble is usually invoke by the `Interactive` behavior.
 */
public class InteractionSpeechBubble : MonoBehaviour
{
    /**
     * Text object displayed in bubble 
     */
    [SerializeField] private TextMeshProUGUI text = default;
    
    /**
     * The bubble sprite
     */
    [SerializeField] private Image image = default;
    /**
     * How long the bubble shall stay at max visibility before starting to fade
     */
    [SerializeField] private float visibleTime = 2.0f;
    /**
     * Duration of fading animation
     */
    [SerializeField] private float fadeTime = 0.5f;
    /**
     * The alpha value the bubble starts with when being invoked
     */
    [SerializeField] private float maxAlpha = 1.0f;

    /**
     * All elements of the bubble are in this canvas group.
     * It is used to control the alpha value of all these elements at once.
     */
    private CanvasGroup _canvasGroup;

    /**
     * Counts down the time until when the timer shall start to fade away.
     * A value of negative infinity means that the timer is not currently running.
     */
    private float _visibleTimer = float.NegativeInfinity;
    /**
     * Counts down the time from the point when the bubble starts to fade away until it completely faded.
     * A value of negative infinity means that the timer is not currently running.
     */
    private float _fadeTimer = float.NegativeInfinity;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start with an invisible speech bubble
        _canvasGroup.alpha = 0;
    }

    private void Update()
    {
        // If the visibility timer is running, count it down
        if (_visibleTimer > 0)
        {
            _visibleTimer -= Time.deltaTime;
        }

        // If the visibility timer is running but has just run out of time, disable it and start the fading timer instead
        else if (!float.IsNegativeInfinity(_visibleTimer))
        {
            _visibleTimer = float.NegativeInfinity;
            _fadeTimer = fadeTime;
        }

        // If the fading timer is running...
        if (_fadeTimer > 0)
        {
            // ...count it down and...
            _fadeTimer -= Time.deltaTime;
            // ...linearly interpolate the alpha value towerds 0 depending on the current value of the timer.
            _canvasGroup.alpha =
                Mathf.Lerp(0, maxAlpha, Mathf.Max(0, _fadeTimer) / fadeTime);
        }

        // If the fading timer is running but has just run out of time, disable it.
        // The speech bubble has completely faded now.
        else if (!float.IsNegativeInfinity(_fadeTimer))
        {
            _fadeTimer = float.NegativeInfinity;
        }
    }

    /**
     * Displays a speech bubble giving a hint about a button which can be pressed, see also class description.
     */
    public void DisplayBubble(Button button)
    {
        // If the appearance of the bubble has been configured for this button...
        if (colorSettings.TryGetValue(button, out var value))
        {
            var (text, color) = value;
            
            // ...display the configured text and color
            this.text.SetText(text);
            image.color = color;

            // ...make the speech bubble visible
            _canvasGroup.alpha = maxAlpha;
            // ...and start the timer counting down the time it will stay visible
            _visibleTimer = visibleTime;
        }
    }

    /**
     * If a speech bubble is currently being displayed, hide it again by making it fade away.
     */
    public void HideBubble()
    {
        // If the timer managing the fade animation is not currently running, start it.
        if (float.IsNegativeInfinity(_fadeTimer) && !float.IsNegativeInfinity(_visibleTimer))
        {
            _fadeTimer = fadeTime;
        }
        _visibleTimer = float.NegativeInfinity;
    }

    /**
     * Stores information on how the speech bubble shall appear for every kind of button.
     *
     * The key of the dictionary must be the button type. Its value is a pair containing the text to display and the
     * color of the speech bubble.
     */
    private static readonly Dictionary<Button, (string, Color)> colorSettings =
        new Dictionary<Button, (string, Color)>()
        {
            {Button.Carry, ("Carry", new Color(0.298f, 0.686f, 0.314f))},
            {Button.Pull, ("Pull", new Color(0.01171875f, 0.60546875f, 0.89803921568627450980f))},
            {Button.Press, ("Press", new Color(0.957f, 0.318f, 0.118f))},
            {Button.Read, ("Read", new Color(1f, 0.702f, 0f))},
            {Button.Open, ("Open", new Color(0.76f, 0.094f, 0.357f))},
            {Button.Attack, ("Attack", new Color(0.898f, 0.224f, 0.208f))},
            {Button.JumpBack, ("Jump", new Color(0.408f, 0.624f, 0.012f))}
        };
}
