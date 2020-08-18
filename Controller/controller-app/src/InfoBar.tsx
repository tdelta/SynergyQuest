import React from 'react';
import { PlayerInfo } from 'controller-client-lib';

import GoldSymbol from './gfx/gold_large.png';
import HeartSymbol from './gfx/heart_large.png';
import EmptyHeartSymbol from './gfx/empty_heart_large.png';

import _ from 'lodash';

export class InfoBar extends React.Component<InfoBarProps, InfoBarState> {
  render() {
    if (!this.props.playerInfo) {
      // No information has been sent yet
      return (
        <button className='pixelButton text no-click' id='infoBar'>
          &nbsp;
        </button>
      );
    }

    let healthPoints = this.props.playerInfo.HealthPoints;
    if (healthPoints < 0) {
      healthPoints = 0;
    }

    return (
      <button className='pixelButton text no-click' id='infoBar'>
        <div style={{ display: 'inline-flex' }}>
          <img className='textImage' src={GoldSymbol} alt='Coins' />
          <div style={{ margin: '3pt' }}>{this.props.playerInfo.Gold}</div>
        </div>
        <div>
          {_.times(healthPoints, () => (
            <img className='textImage' src={HeartSymbol} alt='Heart' />
          ))}
          {_.times(5 - healthPoints, () => (
            <img
              className='textImage'
              src={EmptyHeartSymbol}
              alt='Empty Heart'
            />
          ))}
        </div>
      </button>
    );
  }
}

interface InfoBarProps {
  playerInfo?: PlayerInfo;
}

interface InfoBarState {}
