// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2021
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

using JetBrains.Annotations;
using Settings;
using UnityEditor;
using UnityEngine.Tilemaps;
using Utils;

namespace Editor.Validation.Issues
{
    public class IncorrectMaterialCheck : Check
    {
        [CanBeNull]
        public Issue PerformCheck()
        {
            var tilemap = TilemapExtensions.FindMainTilemap();

            if (tilemap.TryGetComponent(out TilemapRenderer renderer))
            {
                if (renderer.sharedMaterial.name.Replace(" (Instance)", "") != MaterialSettings.Instance.TilemapMaterial.name)
                {
                    return new IncorrectMaterialIssue(renderer);
                }
            }

            return null;
        }
        
        private class IncorrectMaterialIssue : Issue
        {
            public IncorrectMaterialIssue(TilemapRenderer renderer)
            {
                this.renderer = renderer;
            }
                
            private TilemapRenderer renderer;
                
            public string Description => "The tilemap must use the correct tilemap material from the MaterialSettings";
            public bool CanAutofix => true;
            public void Autofix()
            {
                renderer.material = MaterialSettings.Instance.TilemapMaterial;
                EditorUtility.SetDirty(renderer);
            }
        }
    }
}