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

public class Ranking
{
    public int Rank;
    public string Name;
    public int Gold;
}

public class PlayerScoreScroll : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankingText = default;
    [SerializeField] private TextMeshProUGUI playerName = default;
    [SerializeField] private TextMeshProUGUI score = default;
    
    public void Init(Ranking ranking)
    {
        var rankingSuffix = "th";
        var rankingColor = new Color32(66, 66, 66, 255);
        switch (ranking.Rank)
        {
            case 1:
                rankingSuffix = "st";
                rankingColor = new Color32(180, 151, 0, 255);
                break;
            case 2:
                rankingSuffix = "nd";
                rankingColor = new Color32(158, 151, 147, 255);
                break;
            case 3:
                rankingSuffix = "rd";
                rankingColor = new Color32(192, 103, 77, 255);
                break;
        }
        
        rankingText.SetText($"{ranking.Rank}{rankingSuffix}");
        rankingText.color = rankingColor;
        playerName.SetText(ranking.Name);
        score.SetText(ranking.Gold.ToString());
        
        playerName.ForceMeshUpdate();
        playerName.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, playerName.textBounds.size.x);
    }
}
