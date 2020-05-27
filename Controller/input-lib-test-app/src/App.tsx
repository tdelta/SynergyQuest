import React from 'react';
import { Button, ControllerInputSender } from 'controller-input-lib';

/**
 * The following interfaces form an ADT to track the state of the connection
 */
interface NotConnected {
  kind: "NotConnected"
};
const NotConnectedC: NotConnected = {kind: "NotConnected"};

interface Connecting {
  kind: "Connecting"
};
const ConnectingC: Connecting = {kind: "Connecting"};

interface Connected {
  kind: "Connected",
  sender: ControllerInputSender
};
function ConnectedC(s: ControllerInputSender): Connected {
  return {
    kind: "Connected",
    sender: s
  };
};

type ConnectionStatus = NotConnected | Connecting | Connected;

export interface AppState {
  connectionStatus: ConnectionStatus;
}

/**
 * Main UI class
 */
class App extends React.Component<{}, AppState> {
  // References to two sliders which serve as joystick input
  private vertRef = React.createRef<HTMLInputElement>();
  private horRef = React.createRef<HTMLInputElement>();

  constructor(props: {}) {
    super(props);

    this.connect = this.connect.bind(this);
    this.handleInputChange = this.handleInputChange.bind(this);

    // Initialize as not connected
    this.state = {
      connectionStatus: NotConnectedC
    };
  }

  /**
   * Calls the library to establish a connection to the game.
   * Updates the connection state accordingly.
   *
   * Is called when the Connect button is pressed.
   */
  connect(e: React.FormEvent<HTMLFormElement>) {
    // Don't do whatever happens when a form is submitted.
    // We have out custom javascript here instead.
    e.preventDefault();
    // Get a map from input names to their values in the form
    let data = new FormData(e.target as HTMLFormElement)

    // Update the state to "Connecting"
    this.setState({
      connectionStatus: ConnectingC
    });

    // Try to establish a connection
    ControllerInputSender.connect(
      data.get('address') as string,
      +data.get('port')!
    )
    .then( // As soon as a connection has been established, update the state
           // with the initialized sender object.
        sender => this.setState({
          connectionStatus: ConnectedC(sender)
        })
    );
  }

  /**
   * Called whenever one of the inputs changes (Attack, Pull, ...)
   */
  handleInputChange(e:  React.ChangeEvent) {
    if (this.state.connectionStatus.kind === "Connected") {
      let sender = this.state.connectionStatus.sender;
      let node = e.target as any;
      let name = node.name;

      switch(name) {
        case "attack":
          sender.setButton(Button.Attack, node.checked);
          break;
        case "pull":
          sender.setButton(Button.Pull, node.checked);
          break;
        case "vertical":
        case "horizontal":
          let vertVal = this.vertRef.current?.value as any;
          let horVal = this.horRef.current?.value as any;

          sender.setJoystickPosition(vertVal, horVal);
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
        body = <form onSubmit={this.connect}>
          <label>
            Address: <input name="address" defaultValue={window.location.hostname}/>
              </label><br/>
          <label>
            Port:    <input name="port" type="number" defaultValue="4242"/>
          </label><br/>
          <input type="submit" value="Connect"/>
        </form>;
        break;
      case "Connecting":
        body = <span>Connecting...</span>
        break;
      case "Connected":
        body = <span>Y</span>;
        body = <p>
          Attack:             <input name="attack" type="checkbox" onChange={this.handleInputChange}/><br/>
          Pull:               <input name="pull" type="checkbox" onChange={this.handleInputChange}/><br/>
          Joystick Vertical:  <input name="vertical" ref={this.vertRef} type="range" min="-1" max="1" defaultValue="0" step="0.05" onChange={this.handleInputChange}/><br/>
          Joystick Horizontal:  <input name="horizontal" ref={this.horRef} type="range" min="-1" max="1" defaultValue="0" step="0.05" onChange={this.handleInputChange}/>
        </p>;
        break;
    }

    return (
      <div className="App">
        {body}
      </div>
    );
  }
}

export default App;
