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
 * Singleton which allows to launch and control the info screen UI.
 * It also pauses the game and changes the state of remote controllers appropriately
 * (by enabling / disabling menu actions etc.)
 *
 * An info screen is a set of pages with information displayed to players, for example when reading a sign.
 *
 * For the actual info screen UI behavior see the `InfoScreenUi` class.
 * The UI prefab used by this singleton can be set in the `MenuPrefabSettings` instance in the `Resources` folder.
 *
 * !!! See also the base class `MenuLauncher` for a description of the inherited methods.
 */
public class InfoScreenLauncher: MenuLauncher<InfoScreenLauncher, InfoScreenUi>
{
    private bool _isShowingInfoScreen = false;
    public bool IsShowingInfoScreen => _isShowingInfoScreen;

    /**
     * Opens an info screen and pauses the game.
     *
     * @param content the content that shall be displayed by the info screen.
     */
    public void Launch(InfoScreenContent content)
    {
        Launch(ui => ui.Init(content));
    }
    
    protected override InfoScreenUi GetUiPrefab()
    {
        return MenuPrefabSettings.Instance.infoScreenUiPrefab;
    }
    
    protected override void OnUiLaunched()
    {
        _isShowingInfoScreen = true;
    }

    protected override void OnUiClosed()
    {
        _isShowingInfoScreen = false;
    }
}
