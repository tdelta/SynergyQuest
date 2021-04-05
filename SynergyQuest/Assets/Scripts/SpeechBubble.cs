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

using CameraUtils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * <summary>
 * Allows to display a speech bubble with a custom text.
 * </summary>
 * <remarks>
 * Don't directly assign to an object. Instead use the static method <see cref="Display"/> to create and display a
 * speech bubble.
 * This class uses the prefab set in <see cref="PrefabSettings"/>.
 * </remarks>
 */
[RequireComponent(typeof(CameraTracked))]
public class SpeechBubble : MonoBehaviour
{
    [Tooltip("The text of the speech bubble will be displayed using this object.")]
    [SerializeField] private TextMeshProUGUI text = default;
    [Tooltip("All parent layout groups wrapping the text sorted from inside to outside. Will be rebuilt when text changes.")]
    [SerializeField] private LayoutGroup[] layoutGroups = default;

    private CameraTracked _cameraTracked;

    /**
     * <summary>
     * Instantiates and displays a speech bubble.
     * </summary>
     * <param name="position">Where the speech bubble will be displayed (lower left corner)</param>
     * <param name="text">Text to display. The rich text features of TextMeshPro (tags) are supported.</param>
     * <param name="duration">How long the speech bubble will be displayed. It will not be destroyed automatically if set to 0 (default)</param>
     * <returns>the created <see cref="SpeechBubble"/> instance</returns>
     */
    public static SpeechBubble Display(Vector3 position, string text, float duration = 0)
    {
        var instance = Instantiate(PrefabSettings.Instance.SpeechBubblePrefab, position, Quaternion.identity);
        
        instance.text.SetText(text);
        // When changing the text, the layout groups need to be rebuild from the innermost to the outermost to adapt to
        // the changed text size
        foreach (var layoutGroup in instance.layoutGroups.SelectNotNull(layoutGroup => layoutGroup.GetComponent<RectTransform>()))
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
        }

        if (duration > 0)
        {
            Destroy(instance.gameObject, duration);
        }

        return instance;
    }

    private void Awake()
    {
        _cameraTracked = GetComponent<CameraTracked>();
    }

    private void Start()
    {
        // Make sure the speech bubble is visible on screen by making the cameras follow it
        _cameraTracked.Radius = ((RectTransform) this.transform).rect.size.MaxComponent();
        _cameraTracked.Tracking = true;
    }

    private void OnDestroy()
    {
        _cameraTracked.Tracking = false;
    }
}
