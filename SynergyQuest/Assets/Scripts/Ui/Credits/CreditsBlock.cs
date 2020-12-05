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

using System.IO;
using System.Linq;
using Data;
using TMPro;
using UnityEngine;
using WebSocketSharp;

/**
 * <summary>
 * Displays an instance of <see cref="ExternalArtCredits.CreditsEntry"/> in the UI.
 * </summary>
 */
public class CreditsBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI authorValue = default;
    [SerializeField] private TextMeshProUGUI fileValue = default;
    [SerializeField] private TextMeshProUGUI licenseValue = default;
    [SerializeField] private TextMeshProUGUI descriptionValue = default;
    [SerializeField] private TextMeshProUGUI modificationsValue = default;
    [SerializeField] private TextMeshProUGUI linkValue = default;

    public void Init(string filePath, ExternalArtCredits.CreditsEntry entry)
    {
        authorValue.SetText(entry.author);
        fileValue.SetText(Path.GetFileName(filePath));
        licenseValue.SetText(entry.license);
        descriptionValue.SetText(entry.description);
        modificationsValue.SetText(entry.modifications.IsNullOrEmpty() ? "None" : entry.modifications);
        
        linkValue.SetText(
            string.Join(
                " ",
                entry.link
                    .Split(' ')
                    .Select(token =>
                        token.StartsWith("http") ?
                            $"<u><link={token}>{token}</link></u>"
                            : token
                    )
                    .ToArray()
            )
        );
    }
}
