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
            canShowMap={this.props.canShowMap}
            showMap={this.props.showMap}
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
  canShowMap: boolean;
  showMap: () => void;
  playerInfo?: PlayerInfo;
}
