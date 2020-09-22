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

/**
 * <summary>
 * Singleton which allows to launch and control the map screen UI.
 * The map screen gives an overview over the dungeon layout.
 * </summary>
 * <remarks>
 * It also pauses the game and changes the state of remote controllers appropriately
 * (by enabling / disabling menu actions etc.)
 *
 * For the actual map screen UI behavior see the <see cref="MapScreenUi"/> class.
 * The UI prefab used by this singleton can be set in the <see cref="MenuPrefabSettings"/> instance in the
 * <c>Resources</c> folder.
 *
 * !!! See also the base class <see cref="MenuLauncher{LauncherType,UiType}"/> for a description of the inherited methods.
 * </remarks>
 */
public class MapScreenLauncher: MenuLauncher<MapScreenLauncher, MapScreenUi>
{
    public void Launch()
    {
        Launch(null);
    }
    
    protected override MapScreenUi GetUiPrefab()
    {
        return MenuPrefabSettings.Instance.MapScreenUiPrefab;
    }
}
