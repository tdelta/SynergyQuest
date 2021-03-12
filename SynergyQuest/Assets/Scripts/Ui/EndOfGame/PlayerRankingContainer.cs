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

using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FlexibleGridLayout))]
public class PlayerRankingContainer : MonoBehaviour
{
    [SerializeField] private PlayerScoreScroll playerScoreScrollPrefab = default;

    private FlexibleGridLayout _flexibleGridLayout;

    private void Awake()
    {
        _flexibleGridLayout = GetComponent<FlexibleGridLayout>();
    }
    
    void Start()
    {
        var playerRankings = PlayerDataKeeper.Instance
            .PlayerDatas
            .OrderByDescending(data => data.GoldCounter)
            .GroupBy(data => data.GoldCounter)
            .SelectMany((group, index) =>
                group.Select(data => new Ranking()
                {
                    Rank = index + 1,
                    Name = data.Name,
                    Gold = data.GoldCounter
                })
            )
            .Take(4)
            .ToArray();

        _flexibleGridLayout.Columns = playerRankings.Length;
        foreach (var playerRanking in playerRankings)
        {
            var playerScroll = Instantiate(playerScoreScrollPrefab, _flexibleGridLayout.transform);
            playerScroll.Init(playerRanking);
        }
        Debug.Log(playerRankings.Length);
        _flexibleGridLayout.enabled = false;
        _flexibleGridLayout.enabled = true;
    }
}
