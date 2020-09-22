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
import './ConnectScreen.css';
import { ReactComponent as Logo } from './gfx/logo_web.svg';
import { DiagnosticsClient } from 'controller-client-lib';
import { diagnosticsPort } from './consts';

export class ConnectScreen extends React.Component<
  ConnectScreenProbs,
  ConnectScreenState
> {
  constructor(props: ConnectScreenProbs) {
    super(props);
    this.state = {
      playerName: '',
    };

    this.joinGame = this.joinGame.bind(this);
    this.onNameChange = this.onNameChange.bind(this);
  }

  componentDidMount() {
    // If we connected before, then our name has been stored...
    const storedName = window.localStorage.getItem('name');
    if (storedName != null) {
      (async () => {
        try {
          // Ask the server, if there are currently players who lost their connection
          const diagnostics = await DiagnosticsClient.getDiagnostics(
            window.location.hostname,
            diagnosticsPort
          );

          // If we are one of the players with the lost connection, we immediately
          // reconnect
          if (diagnostics.playersWithLostConnection.indexOf(storedName) > -1) {
            this.props.connect(storedName);
          } else {
            // Otherwise, we do not automatically reconnect, but we can display
            // the last used name in the input field
            this.setState({
              ...this.state,
              playerName: storedName,
            });
          }
        } catch (e) {
          console.log(`Failed to retrieve game diagnostics: ${e}`);
        }
      })();
    }
  }

  joinGame(e: React.MouseEvent) {
    e.preventDefault();

    this.props.connect(this.state.playerName);
  }

  onNameChange(e: React.FormEvent<HTMLInputElement>) {
    this.setState({
      ...this.state,
      playerName: e.currentTarget.value,
    });
  }

  render() {
    const connectAction = () => this.props.connect(this.state.playerName);

    const canJoin = this.state.playerName !== '';

    const scrollContent1: React.ReactNode = ( // Displayed, when the name has not been entered yet
      <>
        <p> Welcome to Synergy Quest! Please enter your name: </p>
        <form onSubmit={canJoin ? _ => connectAction() : undefined}>
          <input value={this.state.playerName} onChange={this.onNameChange} />
        </form>
        {canJoin && <p> Now press the button below to join the game :) </p>}
      </>
    );

    return (
      <div className='container'>
        <div className='columnContainer'>
          <Logo id='logo' />
          <div id='contentContainer'>
            <div className='scroll'>{scrollContent1}</div>
            {canJoin && (
              <button className='pixelbutton' onClick={connectAction}>
                Join The Game!
              </button>
            )}
          </div>
        </div>
      </div>
    );
  }
}

interface ConnectScreenState {
  playerName: string;
}

interface ConnectScreenProbs {
  connect: (name: string) => void;
}
