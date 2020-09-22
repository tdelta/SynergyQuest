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

/**
 * <summary>
 * Instead of directly respawning, this behaviour turns players into ghosts when respawning due to death, see
 * <see cref="PlayerGhost"/>.
 * </summary>
 * <remarks>
 * This ghost first must be caught by the other players before the player respawns.
 * This minigame is activated by the <see cref="Spawnable.OnRespawn"/> event of <see cref="Spawnable"/>.
 * </remarks>
 * <seealso cref="Spawnable"/>
 * <see cref="PlayerGhost"/>
 */
[RequireComponent(typeof(Spawnable), typeof(PlayerController))]
public class ReviveMinigame : MonoBehaviour
{
    [SerializeField] private GameObject playerGhostPrefab = default;

    private PlayerController _player;
    private Spawnable _spawnable;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        _spawnable = GetComponent<Spawnable>();
    }

    private void OnEnable()
    {
        _spawnable.OnRespawn += OnRespawn;
    }

    private void OnDisable()
    {
        _spawnable.OnRespawn -= OnRespawn;
    }

    private void OnRespawn(Vector3 respawnPosition, Spawnable.RespawnReason reason)
    {
        if (
            reason == Spawnable.RespawnReason.Death &&
            (!DebugSettings.Instance.DebugMode || !DebugSettings.Instance.DisableRevivalMinigame)
        )
        {
            var instance = Instantiate(playerGhostPrefab, respawnPosition, Quaternion.identity)
              .GetComponentInChildren<PlayerGhost>();
            
            instance.Register(_player, transform.position);
        }
    }

    /**
     * <summary>
     * Returns an array of all players who died and who are currently undergoing the minigame (= they are ghosts).
     * </summary>
     */
    public static PlayerController[] GetPlayersInMinigame()
    {
        return FindObjectsOfType<PlayerGhost>()
            .Select(ghost => ghost.Player)
            .ToArray();
    }
}
