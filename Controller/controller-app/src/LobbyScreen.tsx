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

export class LobbyScreen extends React.Component<
  LobbyScreenProbs,
  LobbyScreenState
> {
  render() {
    const canStartGame = this.props.enabledMenuActions.has(
      MenuAction.StartGame
    );

    let scrollContent;
    if (!canStartGame) {
      scrollContent = <>Waiting for more of your friends to join the game...</>;
    } else {
      scrollContent = (
        <>
          If no one else wants to join, press the button below to start the
          game!
        </>
      );
    }

    return (
      <div className='container'>
        <div className='columnContainer'>
          <Logo id='logo' />
          <div id='contentContainer'>
            <div className='scroll'>{scrollContent}</div>
            {canStartGame && (
              <button className='pixelbutton' onClick={this.props.startGame}>
                Start the Game!
              </button>
            )}
          </div>
        </div>
      </div>
    );
  }
}

interface LobbyScreenState {}

interface LobbyScreenProbs {
  enabledMenuActions: Set<MenuAction>;
  startGame: () => void;
}
