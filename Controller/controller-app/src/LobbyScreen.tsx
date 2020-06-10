import React from 'react';
import { MenuAction, ControllerClient } from 'controller-client-lib';
import './ConnectScreen.css';
import {ReactComponent as Logo} from './gfx/logo_web.svg';

export class LobbyScreen extends React.Component<LobbyScreenProbs, LobbyScreenState> {
  constructor(props: LobbyScreenProbs){
    super(props);
  }

  render () {
    let canStartGame = this.props.enabledMenuActions.has(MenuAction.StartGame);

    let scrollContent;
    if (!canStartGame) {
      scrollContent = <>
        Waiting for more of your friends to join the game...
      </>
    }

    else {
      scrollContent = <>
        If no one else wants to join, press the button below to start the game!
      </>
    }

    return (
      <div
        className='container'
      >
      <div className='columnContainer'>
      <Logo id='logo' />
      <div id='contentContainer'>
        <div className='scroll'>
          {scrollContent}
        </div>
        {canStartGame &&
          <button className='pixelbutton' onClick={this.props.startGame}>
            Start the Game!
          </button>
        }
      </div>
      </div>
      </div>
    );
  }
}

interface LobbyScreenState {
}

interface LobbyScreenProbs {
  enabledMenuActions: Set<MenuAction>;
  startGame: () => void;
}
