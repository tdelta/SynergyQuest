import React from 'react';
import {
  GameState,
  Button,
  MenuAction,
  PlayerColor,
  ControllerClient,
  ConnectFailureReason,
} from 'controller-client-lib';
import { ConnectScreen } from './ConnectScreen';
import { LobbyScreen } from './LobbyScreen';
import * as consts from './consts';
import { Controller } from './Controller';
import './App.css';

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
}

/**
 * Main UI class
 */
class App extends React.Component<{}, AppState> {
  // References to two sliders which serve as joystick input
  private vertRef = React.createRef<HTMLInputElement>();
  private horRef = React.createRef<HTMLInputElement>();

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
  };

  constructor(props: {}) {
    super(props);

    this.connect = this.connect.bind(this);
    this.startGame = this.startGame.bind(this);

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
      this.setState({
        ...this.state,
        connectionStatus: ConnectedC(client),
        failureMessage: undefined,
        gameState: client.getGameState(),
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

    client.onSetMenuAction = (action: MenuAction, enabled: boolean) =>
      this.setState({
        ...this.state,
        enabledMenuActions: client.getEnabledMenuActions(),
      });

    client.onGameStateChanged = (state: GameState) =>
      this.setState({
        ...this.state,
        gameState: state,
      });

    client.connect(playerName, window.location.hostname, consts.port);

    // Update the state to "Connecting"
    this.setState({
      connectionStatus: ConnectingC(client),
    });
  }

  startGame() {
    if (this.state.connectionStatus.kind === 'Connected') {
      const client = this.state.connectionStatus.client;

      client.triggerMenuAction(MenuAction.StartGame);
    }
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
        body = <span>Connecting...</span>;
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
            body = <Controller client={this.state.connectionStatus.client}  playerColor={consts.colors[this.state.color]}/>;
            break;
        }
    }

    return body;
  }
}

export default App;
