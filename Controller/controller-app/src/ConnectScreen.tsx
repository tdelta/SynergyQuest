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
        <p> Welcome to Co-Op Dungeon! Please enter your name: </p>
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
