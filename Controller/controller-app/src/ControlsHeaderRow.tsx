import { ControllerClient, PlayerColor } from 'controller-client-lib';
import React from 'react';
import './Controller.css';
import PauseSymbol from './gfx/pause.png';

import * as consts from './consts';

import {
  faCompress,
  faExpand,
  faMapSigns,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { boundClass } from 'autobind-decorator';

@boundClass
export class ControlsHeaderRow extends React.Component<
  ControlsHeaderRowProps,
  ControlsHeaderRowState
> {
  constructor(probs: ControlsHeaderRowProps) {
    super(probs);

    this.state = {};
  }

  render() {
    const maybePlayerColorRaw = this.props.client.getColor();
    let playerColorRaw = PlayerColor.Any;
    if (maybePlayerColorRaw != null) {
      playerColorRaw = maybePlayerColorRaw;
    }

    return (
      <>
        <div className='controllerMenuContainer'>
          <button
            className='controllerMenuItem text no-click'
            style={{
              backgroundColor: '#e0e0e0', // cplayerColor.dark,
              borderColor: '#eeeeee',
              alignItems: 'center',
              backgroundImage: `url(${consts.avatars[playerColorRaw]})`,
              backgroundSize: 'auto 95%',
              backgroundRepeat: 'no-repeat',
              backgroundPosition: 'center',
            }}
          />
          {this.props.canPause && (
            <button
              className='controllerMenuItem text'
              style={{
                backgroundColor: '#c2185b',
                borderColor: '#e91e63',
                backgroundImage: `url(${PauseSymbol})`,
                backgroundSize: 'auto 50%',
                backgroundRepeat: 'no-repeat',
                backgroundPosition: 'center',
              }}
              onClick={_ => this.props.pause()}
            />
          )}
          {this.props.canShowMap && (
            <button
              className='controllerMenuItem text'
              style={{
                backgroundColor: '#c2185b',
                borderColor: '#e91e63',
              }}
              onClick={this.props.showMap}
            >
              <FontAwesomeIcon icon={faMapSigns} />
            </button>
          )}
          <button
            className='controllerMenuItem text'
            style={{
              backgroundColor: '#c2185b',
              borderColor: '#e91e63',
            }}
            onClick={this.props.toggleFullscreen}
          >
            <FontAwesomeIcon
              icon={this.props.isFullscreen ? faCompress : faExpand}
            />
          </button>
        </div>
      </>
    );
  }
}

interface ControlsHeaderRowState {}

interface ControlsHeaderRowProps {
  client: ControllerClient;
  isFullscreen: boolean;
  toggleFullscreen: () => any;
  playerColor: consts.ColorData;
  canPause: boolean;
  pause: () => void;
  canShowMap: boolean;
  showMap: () => void;
}
