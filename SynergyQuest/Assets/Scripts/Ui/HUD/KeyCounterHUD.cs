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

/**
 * Displays the current number of keys the players have collected.
 */
public class KeyCounterHUD : MonoBehaviour
{
    /**
     * Canvas group which contains all elements of this HUD. It is used to make it invisible, iff the number of keys is
     * 0.
     */
    [SerializeField] private CanvasGroup panel = default;
    /**
     * Text which will be set to the current number of collected keys
     */
    [SerializeField] private TextMeshProUGUI counterText = default;

    private void OnEnable()
    {
        PlayerDataKeeper.Instance.OnNumKeysChanged += OnNumKeysChanged;
    }

    private void OnDisable()
    {
        PlayerDataKeeper.Instance.OnNumKeysChanged -= OnNumKeysChanged;
    }

    private void Start()
    {
        OnNumKeysChanged(PlayerDataKeeper.Instance.NumKeys);
    }

    /**
     * Invoked, if the number of keys which the players currently have changes.
     */
    private void OnNumKeysChanged(int numKeys)
    {
        // If the players have no keys, make the HUD invisible
        if (numKeys <= 0)
        {
            panel.alpha = 0;
        }

        else
        {
            // Otherwise, make it visible and display the number of keys
            panel.alpha = 1;
            counterText.SetText($"{numKeys}x");
        }
    }
}
