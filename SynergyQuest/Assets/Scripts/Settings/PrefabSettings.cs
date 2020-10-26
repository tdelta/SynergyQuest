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

using UnityEngine;

/**
 * <summary>
 * This scriptable object singleton allows to store references to prefabs.
 * </summary>
 * <remarks>
 * An instance of this scriptable object should be stored as `Assets/Resources/PrefabSettings` so that this resource
 * can be retrieved at runtime.
 *
 * Please note, that there are also some other settings objects for more specialized prefab settings, e.g.
 * <see cref="MenuPrefabSettings"/>
 * </remarks>
 */
[CreateAssetMenu(fileName = "PrefabSettings", menuName = "ScriptableObjects/PrefabSettings")]
public class PrefabSettings : ScriptableObjectSingleton<PrefabSettings>
{
    /**
     * <seealso cref="SharedHUD"/>
     */
    public SharedHUD SharedHudPrefab => sharedHudPrefab;
    [SerializeField] private SharedHUD sharedHudPrefab = default;
    
    /**
     * <summary>
     * Player character prefab object spawned for every controller. Must have a `PlayerController` component
     * </summary>
     * <remarks>
     * See also <see cref="PlayerDataKeeper"/>, which accesses this scriptable object singleton to instantiate players.
     * </remarks>
     * <seealso cref="PlayerDataKeeper"/>
     */
    public PlayerController PlayerPrefab => playerPrefab;
    [SerializeField] private PlayerController playerPrefab = default;

    /**
     * <summary>
     * Prefab used by <see cref="SpeechBubble"/> to instantiate a speech bubble game object.
     * </summary>
     */
    public SpeechBubble SpeechBubblePrefab => speechBubblePrefab;
    [SerializeField] private SpeechBubble speechBubblePrefab = default;
    
    public WaterReflectionCamera WaterReflectionCameraPrefab => waterReflectionCameraPrefab;
    [SerializeField] private WaterReflectionCamera waterReflectionCameraPrefab = default;

    public ParticleSystem WaterRipplesPrefab => waterRipplesPrefab;
    [SerializeField] private ParticleSystem waterRipplesPrefab = default;
}
