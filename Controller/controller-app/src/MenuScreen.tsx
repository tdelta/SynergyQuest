/**
 * This file is part of the "Synergy Quest" game
 * (github.com/tdelta/SynergyQuest).
 *
 * Copyright (c) 2020
 *   Marc Arnold     (m_o_arnold@gmx.de)
 *   Martin Kerscher (martin_x@live.de)
 *   Jonas Belouadi  (jonas.belouadi@posteo.net)
 *   Anton W Haubner (anton.haubner@outlook.de)
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation; either version 3 of the License, or (at your option) any
 * later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, see <https://www.gnu.org/licenses>.
 *
 * Additional permission under GNU GPL version 3 section 7 apply,
 * see `LICENSE.md` at the root of this source code repository.
 */

import React from 'react';
import { MenuAction } from 'controller-client-lib';
import './ConnectScreen.css';
import { ReactComponent as Logo } from './gfx/logo_web.svg';
import { menuActionStrings } from './EnumStrings';

/**
 * Menu actions are always displayed in this order
 */
const menuActionDisplayOrder = [
  MenuAction.StartGame,
  MenuAction.PauseGame,
  MenuAction.ResumeGame,
  MenuAction.QuitGame,
  MenuAction.Next,
  MenuAction.Back,
  MenuAction.No,
  MenuAction.Yes,
  MenuAction.ShowMap,
];

export class MenuScreen extends React.Component<
  MenuScreenProbs,
  MenuScreenState
> {
  render() {
    let scrollContent = <>Please follow the instructions on the game screen</>;
    if (this.props.enabledMenuActions.size > 0) {
      const menuActions = Array.from(this.props.enabledMenuActions).filter(
        menuAction => menuAction !== MenuAction.ShowMap
      );

      // Sort the available menu actions to display them in the intended order
      menuActions.sort((left, right) => {
        const leftIdx = menuActionDisplayOrder.indexOf(left);
        const rightIdx = menuActionDisplayOrder.indexOf(right);

        return leftIdx - rightIdx;
      });

      const buttonList = Array.from(menuActions).map(action => {
        const click = (_: any) => this.props.triggerMenuAction(action);

        return (
          <>
            <button
              style={{
                marginTop: '10px',
                marginBottom: '10px',
              }}
              className='pixelbutton'
              onClick={click}
            >
              {menuActionStrings.get(action)}
            </button>
            <br />
          </>
        );
      });

      scrollContent = (
        <div style={{ padding: '2em', textAlign: 'center' }}>{buttonList}</div>
      );
    }

    return (
      <div className='container'>
        <div className='columnContainer'>
          <Logo id='logo' />
          <div id='contentContainer'>
            <div className='scroll'>{scrollContent}</div>
          </div>
        </div>
      </div>
    );
  }
}

interface MenuScreenState {}

interface MenuScreenProbs {
  enabledMenuActions: Set<MenuAction>;
  triggerMenuAction: (action: MenuAction) => void;
}
