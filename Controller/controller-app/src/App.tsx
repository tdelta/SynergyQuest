import React from 'react';
import { Button, MenuAction, PlayerColor, ControllerClient, ConnectFailureReason } from 'controller-client-lib';
import { ConnectScreen } from './ConnectScreen';
import * as consts from './consts';
import { Controller } from './Controller';
import './App.css'

/**
 * The following interfaces form an ADT to track the state of the connection
 */
interface NotConnected {
  kind: "NotConnected"
};
const NotConnectedC: NotConnected = {kind: "NotConnected"};

interface Connecting {
  kind: "Connecting",
  client: ControllerClient
};
function ConnectingC(c: ControllerClient): Connecting {
  return {
    kind: "Connecting",
    client: c
  };
};

interface Connected {
  kind: "Connected",
  client: ControllerClient
};
function ConnectedC(c: ControllerClient): Connected {
  return {
    kind: "Connected",
    client: c
  };
};

type ConnectionStatus = NotConnected | Connecting | Connected;

export interface AppState {
  connectionStatus: ConnectionStatus;

  failureMessage?: string;
  color?: PlayerColor;
  attackChecked: boolean;
  pullChecked: boolean;
  horizontalSliderVal: number;
  verticalSliderVal: number;
  enabledMenuActions: Set<MenuAction>;
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
      color: undefined,
      attackChecked: false,
      pullChecked: false,
      horizontalSliderVal: 0,
      verticalSliderVal: 0,
      enabledMenuActions: new Set<MenuAction>()
  };

  constructor(props: {}) {
    super(props);

    this.connect = this.connect.bind(this);
    this.handleInputChange = this.handleInputChange.bind(this);

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
    let client = new ControllerClient();

    client.onReady = () => {
      this.setState({
        ...this.state,
        connectionStatus: ConnectedC(client),
        failureMessage: undefined
      });
    };

    client.onConnectFailure = (reason: ConnectFailureReason) => {
      let failMessage: string;
      switch(reason) {
        case ConnectFailureReason.NameAlreadyTaken:
          failMessage = "Could not connect, since someone with that name is already connected.";
          break;

        case ConnectFailureReason.MaxPlayersReached:
          failMessage = "Could not connect, since the game is already full and no more players can join.";
          break;
      }

      this.setState({
        ...App.initialState,
        failureMessage: failMessage
      });
    };

    client.onDisconnect = () => {
      this.setState({
        ...App.initialState,
        failureMessage: this.state.failureMessage
      });
    };

    client.onError = () => {
      this.setState({
        ...this.state,
        failureMessage: "Some sort of connection error occured."
      });
    };

    client.onSetPlayerColor = (color: PlayerColor) => this.setState({
      ...this.state,
      color: color, // <- set new color
    });

    client.onSetMenuAction = (action: MenuAction, enabled: boolean) => this.setState({
      ...this.state,
      enabledMenuActions: client.getEnabledMenuActions()
    });

    client.connect(
      playerName,
      window.location.hostname,
      consts.port
    );

    // Update the state to "Connecting"
    this.setState({
      connectionStatus: ConnectingC(client)
    });
  }


  /**
   * Called whenever one of the inputs changes (Attack, Pull, ...)
   */
  handleInputChange(e:  React.ChangeEvent) {
    if (this.state.connectionStatus.kind === "Connected") {
      let sender = this.state.connectionStatus.client;
      let node = e.target as any;
      let name = node.name;

      switch(name) {
        case "attack":
          this.setState({...this.state, attackChecked: !this.state.attackChecked});

          sender.setButton(Button.Attack, node.checked);
          break;
        case "pull":
          this.setState({...this.state, pullChecked: !this.state.pullChecked});

          sender.setButton(Button.Pull, node.checked);
          break;
        case "vertical":
        case "horizontal":
          let vertVal = this.vertRef.current?.value as number | undefined;
          let horVal = this.horRef.current?.value as number | undefined;

          if (vertVal != null && horVal != null) {
            this.setState({
              ...this.state,
              horizontalSliderVal: horVal,
              verticalSliderVal: vertVal,
            });

            sender.setJoystickPosition(vertVal, horVal);
          }
      }
    }
  }

  /**
   * Display different HTML, depending on whether we are connected to a game
   * or not.
   */
  render() {
    let body: React.ReactNode;
    switch (this.state.connectionStatus.kind) {
      case "NotConnected":
        body = <ConnectScreen startGame={this.connect} canStartGame={MenuAction.StartGame in this.state.enabledMenuActions}/>
        break;
      case "Connecting":
        body = <span>Connecting...</span>
        break;
      case "Connected":
        body = <Controller client={this.state.connectionStatus.client} />
        break;
    }

    return body;
  }
}

export default App;
