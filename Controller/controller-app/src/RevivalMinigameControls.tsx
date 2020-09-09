import React from 'react';
import './Controller.css';
import './RevivalMinigameControls.css';
import { boundClass } from 'autobind-decorator';
import { ControllerClient, Button, PlayerInfo } from 'controller-client-lib';
import { ControlsHeaderRow } from './ControlsHeaderRow';
import { InfoBar } from './InfoBar';
import * as consts from './consts';
import RIPImage from './gfx/rip.png';

@boundClass
export class RevivalMinigameControls extends React.Component<
  RevivalMinigameControlsProps,
  RevivalMinigameControlsState
> {
  constructor(props: RevivalMinigameControlsProps) {
    super(props);

    this.state = {};
  }

  render() {
    return (
      <div className='container' style={{ userSelect: 'none' }}>
        <InfoBar playerInfo={this.props.playerInfo} />
        <div
          className='rowContainer'
          style={{
            width: '50%',
            height: '12%',
            left: '50%',
            top: '0',
            position: 'absolute',
          }}
        >
          <ControlsHeaderRow
            client={this.props.client}
            isFullscreen={this.props.isFullscreen}
            toggleFullscreen={this.props.toggleFullscreen}
            playerColor={this.props.playerColor}
            canPause={this.props.canPause}
            pause={this.props.pause}
            canExit={this.props.enabledButtons.has(Button.Exit)}
          />
        </div>
        <div className='rowContainer' style={{ height: '88%' }}>
          <div id='ripInfo'>
            <img id='ripImage' src={RIPImage} alt='' />
            <span id='ripHeader'> You died :( </span>
            <span id='ripText'>
              Your team mates need to defeat your vengeful ghost to revive you!
            </span>
          </div>
        </div>
      </div>
    );
  }
}

interface RevivalMinigameControlsState {}

interface RevivalMinigameControlsProps {
  client: ControllerClient;
  isFullscreen: boolean;
  toggleFullscreen: () => Promise<any>;
  playerColor: consts.ColorData;
  enabledButtons: Set<Button>;
  canPause: boolean;
  pause: () => void;
  playerInfo?: PlayerInfo;
}
