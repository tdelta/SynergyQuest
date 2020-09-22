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

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * <summary>
 * Opens hyperlinks in TextMeshPro components on click.
 * </summary>
 * <remarks>
 * Based on https://deltadreamgames.com/unity-tmp-hyperlinks/
 * </remarks>
 */
[RequireComponent(typeof(TextMeshProUGUI))]
public class HyperlinkSupport : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, UnityEngine.Input.mousePosition, null);
        
        // if link was clicked...
        if(linkIndex != -1) {
            var linkInfo = _text.textInfo.linkInfo[linkIndex];
            
            // open the link
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}
