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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * <summary>
 * This scriptable object singleton associates tiles with walking / stepping sounds.
 * </summary>
 * <remarks>
 * These sounds are used by <see cref="WalkSounds"/>.
 * 
 * An instance of this scriptable object should be stored as `Assets/Resources/WalkSoundSettings` so that this resource
 * can be retrieved at runtime.
 * </remarks>
 */
[CreateAssetMenu(fileName = "WalkSoundSettings", menuName = "ScriptableObjects/WalkSoundSettings")]
public class WalkSoundSettings : ScriptableObjectSingleton<WalkSoundSettings>
{
    [Tooltip("Default walk sound to play, if the current tile is not associated with a sound")]
    [SerializeField] private WalkSoundType defaultWalkSound = WalkSoundType.Stone;
    
    [Tooltip("Associates a sound type with a collection of audio clips.")]
    [SerializeField] private WalkSoundCollection[] walkSoundClips = new WalkSoundCollection[0];
    
    [Tooltip("Associates a sound type with a collection of tiles for which it shall be played.")]
    [SerializeField] private WalkSoundTilesAssociation[] walkSoundTilesAssociations = new WalkSoundTilesAssociation[0];

    /**
     * Cache that maps sound types to clip collections
     */
    private Dictionary<WalkSoundType, WalkSoundCollection> _soundTypeToClipsCache = null;
    /*+
     * Cache that maps tile names to sound types
     */
    private Dictionary<string, WalkSoundType> _tileNameToSoundTypeCache = null;

    protected override void OnInstantiate()
    {
        // Initialize caches...
        
        _soundTypeToClipsCache = new Dictionary<WalkSoundType, WalkSoundCollection>();
        foreach (var walkSoundCollection in walkSoundClips)
        {
            _soundTypeToClipsCache.Add(walkSoundCollection.walkSoundType, walkSoundCollection);
        }
        
        _tileNameToSoundTypeCache = new Dictionary<string, WalkSoundType>();
        foreach (var walkSoundTileAssociation in walkSoundTilesAssociations)
        {
            foreach (var tile in walkSoundTileAssociation.tiles)
            {
                _tileNameToSoundTypeCache.Add(tile.name, walkSoundTileAssociation.walkSoundType);
            }
        }
    }

    /**
     * <summary>
     * Default walking audio clips
     * </summary>
     */
    [CanBeNull]
    public AudioClip[] DefaultSounds => _soundTypeToClipsCache.GetOrDefault(defaultWalkSound, null)?.clips;

    /**
     * <summary>
     * Return walking audio clips for a given tile.
     * If the tile is not associated with any sound type, the default sounds are returned.
     * </summary>
     */
    [CanBeNull]
    public AudioClip[] TileToSounds(TileBase tile)
    {
        var soundType = _tileNameToSoundTypeCache.GetOrDefault(tile.name, defaultWalkSound);

        if (_soundTypeToClipsCache.TryGetValue(soundType, out var walkSoundCollection))
        {
            return walkSoundCollection.clips;
        }

        return null;
    }
    
    public enum WalkSoundType
    {
        None = 0,
        Stone = 1,
        Puddle = 2
    }

    [Serializable]
    public class WalkSoundTilesAssociation
    {
        public WalkSoundType walkSoundType = WalkSoundType.None;
        public TileBase[] tiles = new TileBase[0];
    }

    [Serializable]
    public class WalkSoundCollection
    {
        public WalkSoundType walkSoundType;
        public AudioClip[] clips;
    }
}
