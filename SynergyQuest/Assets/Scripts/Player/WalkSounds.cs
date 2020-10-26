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
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * <summary>
 * Plays walking / step sounds for the player depending on the tile they are standing on.
 * </summary>
 * <remarks>
 * The association from tiles to sounds is set in <see cref="WalkSoundSettings"/>.
 * </remarks>
 */
public class WalkSounds : MonoBehaviour
{
    [SerializeField] private AudioSource walkSoundSource = default;

    private Tilemap _tilemap;

    private void Start()
    {
        _tilemap = TilemapExtensions.FindMainTilemap();
    }

    /**
     * <summary>
     * Retrieves the tile with the highest z-Value for the tilemap cell the player is
     * currently standing in.
     * </summary>
     */
    [CanBeNull]
    private TileBase GetCurrentTile()
    {
        var cell = _tilemap.WorldToCell(this.transform.position);
        var tilemapBounds = _tilemap.cellBounds;
        var bounds = new BoundsInt(
            cell.x,
            cell.y,
            tilemapBounds.zMin,
            1,
            1,
            tilemapBounds.size.z
        );
        
        var tiles = _tilemap.GetTilesBlock(bounds);

        return tiles.Reverse().FirstOrDefault(tile => tile != null);
    }

    /**
     * <summary>
     * Randomly selects an audioclip from an array of clips.
     * </summary>
     */
    [CanBeNull]
    private AudioClip SelectClip([CanBeNull] AudioClip[] clips)
    {
        if (clips != null && clips.Any())
        {
            var sound = clips[Random.Range(0, clips.Length)];

            return sound;
        }

        return null;
    }

    /**
     * <summary>
     * Plays a stepping sound. Called by player animation events.
     * </summary>
     */
    private void HandleFootDown()
    {
        var tile = GetCurrentTile();

        AudioClip[] clips;
        if (tile != null)
        {
            clips = WalkSoundSettings.Instance.TileToSounds(tile);
        }

        else
        {
            clips = WalkSoundSettings.Instance.DefaultSounds;
        }

        var clip = SelectClip(clips);
        if (clip != null)
        {
            walkSoundSource.PlayOneShot(clip);
        }
    }
}
