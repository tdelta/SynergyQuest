import React from 'react';
//import { Button as ControllerButton} from 'controller-client-lib';
import './ConnectScreen.css';
import {ReactComponent as Logo} from './gfx/logo_web.svg';

export class ConnectScreen extends React.Component<ConnectScreenProbs, ConnectScreenState> {

  private nameInput: React.RefObject<HTMLInputElement>;

  constructor(props: ConnectScreenProbs){
    super(props);
    this.state = {
      nameEntered: false,
      name: ''
    }

    this.nameInput = React.createRef();

    this.joinGame = this.joinGame.bind(this);
  }

  joinGame(e: React.MouseEvent){
    e.preventDefault();
    if(!this.state.nameEntered) {
      // We are currently on the screen, where a name can be entered
      if(this.nameInput.current == null){
        (console.error || console.log).call("Join game was clicked, but name input field is not on screen")
      } else {
        var name = this.nameInput.current.value;
        this.setState({
          'nameEntered': true,
          'name': name 
        });
      }
    }
    else {
      // Name has been entered and user clicked start game
      this.props.startGame(this.state.name);
    }
  }

  render () {
    const scrollContent1: React.ReactNode = // Displayed, when the name has not been entered yet 
        <>
          <p> Welcome to Co-Op Dungeon! Please enter your name: </p>
          <input ref={this.nameInput} id='nameInput'></input>
          <p> Now press the button below to join the game :) </p>
        </>;
    const scrollContent2: React.ReactNode = // Displayed after the name was entered
        <p> Are all players ready? If so, press the button below to start the game. </p>;

    const buttonContent1: string = 'Join The Game!'; // Displayed when the name has not been entered yet
    const buttonContent2: string = 'Start The Game!'; // Displayed after the name was entered

    return (
      <div
        className='container'
      >
      <div className='columnContainer'>
      <Logo id='logo' />
      <div id='contentContainer'>
        <div className='scroll'>
          {this.state.nameEntered ? scrollContent2 : scrollContent1}
        </div>
        <button className='pixelbutton' onClick={this.joinGame} disabled={this.props.canStartGame}>
          {this.state.nameEntered ? buttonContent2 : buttonContent1}
        </button>
      </div>
      </div>
      </div>
    );
  }
}

interface ConnectScreenState {
  nameEntered: boolean;
  name: string;
}

interface ConnectScreenProbs {
  startGame: (playerName: string) => void;
  canStartGame: boolean;
}
