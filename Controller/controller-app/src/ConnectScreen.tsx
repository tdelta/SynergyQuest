import React from 'react';
//import { Button as ControllerButton} from 'controller-client-lib';
import './ConnectScreen.css';
import {ReactComponent as Logo} from './gfx/logo_web.svg';

export class ConnectScreen extends React.Component<ConnectScreenProbs, ConnectScreenState> {
  constructor(props: ConnectScreenProbs){
    super(props);
    this.state = {
      playerName: ''
    }

    this.joinGame = this.joinGame.bind(this);
    this.onNameChange = this.onNameChange.bind(this);
  }

  joinGame(e: React.MouseEvent){
    e.preventDefault();

    this.props.connect(this.state.playerName);
  }

  onNameChange(e: React.FormEvent<HTMLInputElement>) {
    this.setState({
      ...this.state,
      playerName: e.currentTarget.value
    });
  }

  render () {
    const canJoin = this.state.playerName != '';

    const scrollContent1: React.ReactNode = // Displayed, when the name has not been entered yet 
        <>
          <p> Welcome to Co-Op Dungeon! Please enter your name: </p>
          <input
            value={this.state.playerName}
            onChange={this.onNameChange}
          />
          {canJoin &&
            <p> Now press the button below to join the game :) </p>
          }
        </>;

    return (
      <div
        className='container'
      >
      <div className='columnContainer'>
      <Logo id='logo' />
      <div id='contentContainer'>
        <div className='scroll'>
          {scrollContent1}
        </div>
        {canJoin &&
          <button className='pixelbutton' onClick={() => this.props.connect(this.state.playerName)}>
            Join The Game!
          </button>
        }
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
  connect: (name: string) => void
}
