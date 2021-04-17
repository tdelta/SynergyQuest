/**
 * This file is part of the "Synergy Quest" game
 * (github.com/tdelta/SynergyQuest).
 *
 * Copyright (c) 2020
 *   Marc Arnold     (m_o_arnold@gmx.de)
 *   Martin Kerscher (martin_x@live.de)
 *   Jonas Belouadi  (jonas.belouadi@posteo.net)
 *   Anton W Haubner (anton.haubner@outlook.de)
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation; either version 3 of the License, or (at your option) any
 * later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, see <https://www.gnu.org/licenses>.
 *
 * Additional permission under GNU GPL version 3 section 7 apply,
 * see `LICENSE.md` at the root of this source code repository.
 */

import React from 'react';
import { PlayerInfo } from 'controller-client-lib';

import GoldSymbol from './gfx/gold_large.png';
import HeartSymbol from './gfx/heart_large.png';
import EmptyHeartSymbol from './gfx/empty_heart_large.png';
import Crack from './gfx/crack.png';

import _ from 'lodash';

export class InfoBar extends React.PureComponent<InfoBarProps, InfoBarState> {
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
      <div className='leftBorders text' id='infoBar'>
        <div style={{ display: 'inline-flex' }}>
          <img className='textImage' src={GoldSymbol} alt='Coins' />
          <div style={{
            margin: '0.1em',
            marginTop: '0.2em',
          }}>{this.props.playerInfo.Gold}</div>
        </div>
        {healthPoints === 1 &&
          <img 
            src={Crack} 
            style={{ height:'100%'}}
            alt='Cracked Stone' />
        }     
        <div style={{display: 'inline' }}>
          {_.times(5 - healthPoints, () => (
            <img
              className='textImage'
              src={EmptyHeartSymbol}
              alt='Empty Heart'
            />
          ))}
          {_.times(healthPoints, () => (
            <img className='textImage' src={HeartSymbol} alt='Heart' />
          ))}
        </div>
      </div>
    );
  }
}

interface InfoBarProps {
  playerInfo?: PlayerInfo;
}

interface InfoBarState {}
