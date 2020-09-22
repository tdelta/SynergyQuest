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
import {
  Button,
  ConnectFailureReason,
  ControllerClient,
  GameState,
  MenuAction,
  PlayerColor,
  InputMode,
  PlayerInfo,
} from 'controller-client-lib';
import { ConnectScreen } from './ConnectScreen';
import { LobbyScreen } from './LobbyScreen';
import { MenuScreen } from './MenuScreen';
import * as consts from './consts';
import { Controller } from './Controller';
import { LoadingScreen } from './LoadingScreen';
import './App.css';
import { boundClass } from 'autobind-decorator';

/**
 * The following interfaces form an ADT to track the state of the connection
 */
interface NotConnected {
  kind: 'NotConnected';
}
const NotConnectedC: NotConnected = { kind: 'NotConnected' };

interface Connecting {
  kind: 'Connecting';
  client: ControllerClient;
}
function ConnectingC(c: ControllerClient): Connecting {
  return {
    kind: 'Connecting',
    client: c,
  };
}

interface Connected {
  kind: 'Connected';
  client: ControllerClient;
}
function ConnectedC(c: ControllerClient): Connected {
  return {
    kind: 'Connected',
    client: c,
  };
}

type ConnectionStatus = NotConnected | Connecting | Connected;

export interface AppState {
  connectionStatus: ConnectionStatus;

  failureMessage?: string;
  color: PlayerColor;
  attackChecked: boolean;
  pullChecked: boolean;
  horizontalSliderVal: number;
  verticalSliderVal: number;
  enabledMenuActions: Set<MenuAction>;
  gameState: GameState;
  inputMode: InputMode;
  enabledButtons: Set<Button>;
  cooldownButtons: Set<Button>;
  playerInfo?: PlayerInfo;
}

/**
 * Main UI class
 */
@boundClass
class App extends React.Component<{}, AppState> {
  private static readonly initialState: AppState = {
    connectionStatus: NotConnectedC,
    failureMessage: undefined,
    color: PlayerColor.Any,
    attackChecked: false,
    pullChecked: false,
    horizontalSliderVal: 0,
    verticalSliderVal: 0,
    enabledMenuActions: new Set<MenuAction>(),
    gameState: GameState.Lobby,
    inputMode: InputMode.Normal,
    enabledButtons: new Set<Button>(),
    cooldownButtons: new Set<Button>(),
    playerInfo: undefined,
  };

  constructor(props: {}) {
    super(props);

    // Initialize as not connected
    this.state = App.initialState;
  }

  /**
   * Calls the library to establish a connection to the game.
   * Updates the connection state accordingly.
   *
   * Is called when the Connect button is pressed.
   */
  connect(playerName: string) {
    const client = new ControllerClient();

    client.onReady = () => {
      // If we connected successfully, we remember the name we connected with.
      // This way, `ConnectScreen` can use this name to reconnect when reloading
      // the page after having lost the connection.
      window.localStorage.setItem('name', playerName);

      this.setState({
        ...this.state,
        connectionStatus: ConnectedC(client),
        failureMessage: undefined,
        gameState: client.getGameState(),
        inputMode: client.getInputMode(),
      });
    };

    client.onConnectFailure = (reason: ConnectFailureReason) => {
      let failMessage: string;
      switch (reason) {
        case ConnectFailureReason.NameAlreadyTaken:
          failMessage =
            'Could not connect, since someone with that name is already connected.';
          break;

        case ConnectFailureReason.MaxPlayersReached:
          failMessage =
            'Could not connect, since the game is already full and no more players can join.';
          break;
      }

      this.setState({
        ...App.initialState,
        failureMessage: failMessage,
      });
    };

    client.onDisconnect = () => {
      this.setState({
        ...App.initialState,
        failureMessage: this.state.failureMessage,
      });
    };

    client.onError = () => {
      this.setState({
        ...this.state,
        failureMessage: 'Some sort of connection error occured.',
      });
    };

    client.onSetPlayerColor = (color: PlayerColor) =>
      this.setState({
        ...this.state,
        color: color, // <- set new color
      });

    client.onSetEnabledMenuActions = _ =>
      this.setState({
        ...this.state,
        enabledMenuActions: client.getEnabledMenuActions(),
      });

    client.onSetEnabledButtons = _ =>
      this.setState({
        enabledButtons: client.getEnabledButtons(),
      });

    client.onSetCooldownButtons = _ =>
      this.setState({
        cooldownButtons: client.getCooldownButtons(),
      });

    client.onGameStateChanged = (state: GameState) =>
      this.setState({
        ...this.state,
        gameState: state,
      });

    client.onInputModeChanged = (inputMode: InputMode) =>
      this.setState({
        ...this.state,
        inputMode: inputMode,
      });

    client.onSetPlayerInfo = (playerInfo: PlayerInfo) =>
      this.setState({
        playerInfo: playerInfo,
      });

    client.onVibrationRequest = (vibrationPattern: number[]) =>
      window.navigator.vibrate(vibrationPattern);

    client.connect(playerName, window.location.hostname, consts.port);

    // Update the state to "Connecting"
    this.setState({
      connectionStatus: ConnectingC(client),
    });
  }

  startGame() {
    this.triggerMenuAction(MenuAction.StartGame);
  }

  pause() {
    this.triggerMenuAction(MenuAction.PauseGame);
  }

  showMap() {
    this.triggerMenuAction(MenuAction.ShowMap);
  }

  triggerMenuAction(action: MenuAction) {
    if (this.state.connectionStatus.kind === 'Connected') {
      const client = this.state.connectionStatus.client;

      client.triggerMenuAction(action);
    }
  }

  displayFailure(message: string) {
    this.setState({
      ...this.state,
      failureMessage: message,
    });
  }

  /**
   * Display different HTML, depending on whether we are connected to a game
   * or not.
   */
  render() {
    let body: React.ReactNode;
    switch (this.state.connectionStatus.kind) {
      case 'NotConnected':
        body = <ConnectScreen connect={this.connect} />;
        break;
      case 'Connecting':
        body = <LoadingScreen />;
        break;
      case 'Connected':
        switch (this.state.gameState) {
          case GameState.Lobby:
            body = (
              <LobbyScreen
                enabledMenuActions={this.state.enabledMenuActions}
                startGame={this.startGame}
              />
            );
            break;
          case GameState.Started:
            body = (
              <Controller
                client={this.state.connectionStatus.client}
                inputMode={this.state.inputMode}
                playerColor={consts.colors[this.state.color]}
                enabledButtons={this.state.enabledButtons}
                cooldownButtons={this.state.cooldownButtons}
                canPause={this.state.enabledMenuActions.has(
                  MenuAction.PauseGame
                )}
                pause={this.pause}
                canShowMap={this.state.enabledMenuActions.has(
                  MenuAction.ShowMap
                )}
                showMap={this.showMap}
                displayFailure={this.displayFailure}
                playerInfo={this.state.playerInfo}
              />
            );
            break;
          case GameState.Menu:
            body = (
              <MenuScreen
                enabledMenuActions={this.state.enabledMenuActions}
                triggerMenuAction={this.triggerMenuAction}
              />
            );
            break;
        }
    }

    if (this.state.failureMessage) {
      // Draw the error on top of the other elements
      body = (
        <>
          <div className='errorMessage text'>{this.state.failureMessage}</div>
          {body}
        </>
      );
    }

    return body;
  }
}

export default App;
