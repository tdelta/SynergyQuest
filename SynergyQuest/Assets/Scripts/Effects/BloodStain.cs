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
using Utils;

namespace Effects
{
    /**
     * <summary>
     * Renders a random blood stain sprite on the floor.
     * </summary>
     */
    [RequireComponent(typeof(SpriteRenderer))]
    public class BloodStain : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The sprites of the blood stains. Will be randomly selected. They should be grayscale so that they can be colored with the given blood color value.")]
        private Sprite[] bloodStainSprites = new Sprite[0];
        
        /**
         * <summary>The color of the blood.</summary>
         */
        public Color color = new Color(1, 0.149f, 0, 1);

        private SpriteRenderer _renderer = default;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            RandomExtensions
                .SelectUniform(bloodStainSprites)
                .Match(
                    some: sprite => _renderer.sprite = sprite,
                    none: () => Debug.LogError("No blood stain sprites available.")
                );

            _renderer.color = color;
        }
    }
}