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
