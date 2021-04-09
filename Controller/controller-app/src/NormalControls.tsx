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

import { Button, IControllerClient, PlayerInfo } from 'controller-client-lib';
import React from 'react';
import nipplejs, {
  JoystickManager,
  JoystickManagerOptions,
  EventData,
  JoystickOutputData,
} from 'nipplejs';
import './Controller.css';

import * as consts from './consts';

import { boundClass } from 'autobind-decorator';
import { ControlsHeaderRow } from './ControlsHeaderRow';
import { InfoBar } from './InfoBar';
import { ControllerButtons } from './ControllerButtons';

@boundClass
export class NormalControls extends React.Component<
  NormalControlsProps,
  NormalControlsState
> {
  private boob: React.RefObject<HTMLDivElement>;

  constructor(props: NormalControlsProps) {
    super(props);

    this.boob = React.createRef();
    this.state = {};
  }

  onButtonChanged(button: Button, pressed: boolean) {
    this.props.client.setButton(button, pressed);
  }

  onJoystickMoved(_evt: EventData, data: JoystickOutputData) {
    const threshold: number = 0.2;

    const force: number = Math.min(data.force * 0.6, 1);

    let horizontal: number;
    let vertical: number;
    if (data.force > threshold) {
      const angle: number = data.angle.radian;

      horizontal = Math.cos(angle) * force;
      vertical = Math.sin(angle) * force;
    } else {
      horizontal = 0;
      vertical = 0;
    }

    this.props.client.setJoystickPosition(vertical, horizontal);
  }

  onJoystickReleased(_evt: EventData, _data: JoystickOutputData) {
    this.props.client.setJoystickPosition(0, 0);
  }

  componentDidMount() {
    // Create joystick on div "boob"
    const boob = this.boob.current!; // Boob definetly exists when componentDidMount is called
    const options: JoystickManagerOptions = {
      zone: boob,
    };
    const manager: JoystickManager = nipplejs.create(options);
    manager.on('move', this.onJoystickMoved);
    manager.on('end', this.onJoystickReleased);
  }

  render() {
    // Return DOM elements
    return (
      <div className='container'>
        <div className='rowContainer' style={{ userSelect: 'none' }}>
          <div className='columnContainer'>
            <div className='gameControls'>
              <div className='columnContainer' style={{ width: '50%' }}>
                <InfoBar playerInfo={this.props.playerInfo} />
                <div id='boob' className='text' ref={this.boob}>
                  <p id='boobText'> tap and drag to move </p>
                </div>
              </div>
              <div className='rightColumn'>
                <div className='rowContainer' style={{ height: '12%' }}>
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
                <ControllerButtons
                  enabledButtons={this.props.enabledButtons}
                  cooldownButtons={this.props.cooldownButtons}
                  onButtonChanged={this.onButtonChanged}
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }
}

interface NormalControlsState {}

interface NormalControlsProps {
  client: IControllerClient;
  isFullscreen: boolean;
  enabledButtons: Set<Button>;
  cooldownButtons: Set<Button>;
  toggleFullscreen: () => Promise<any>;
  playerColor: consts.ColorData;
  canPause: boolean;
  pause: () => void;
  canShowMap: boolean;
  showMap: () => void;
  playerInfo?: PlayerInfo;
}
