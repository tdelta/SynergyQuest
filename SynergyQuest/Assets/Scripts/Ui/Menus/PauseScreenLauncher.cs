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
 * Singleton which allows to launch and control the pause screen UI and pause the game.
 * It also changes the state of remote controllers appropriately
 * (by enabling / disabling menu actions etc.)
 *
 * For the actual pause screen UI behavior see the `PauseScreenUi` class.
 * The UI prefab used by this singleton can be set in the `MenuPrefabSettings` instance in the `Resources` folder.
 *
 * !!! See also the base class `MenuLauncher` for a description of the inherited methods.
 */
public class PauseScreenLauncher: MenuLauncher<PauseScreenLauncher, PauseScreenUi>
{
    private bool _isPaused = false;
    public bool IsPaused => _isPaused;
    
    /**
     * Opens the pause screen and pauses the game.
     */
    public void Launch()
    {
        // We can directly call the base class method without a callback, since the pause screen UI behavior does not
        // need any special initialization steps
        Launch(null);
    }
    
    protected override PauseScreenUi GetUiPrefab()
    {
        return MenuPrefabSettings.Instance.pauseScreenUiPrefab;
    }

    protected override void OnUiLaunched()
    {
        _isPaused = true;
    }

    protected override void OnUiClosed()
    {
        _isPaused = false;
    }
}
