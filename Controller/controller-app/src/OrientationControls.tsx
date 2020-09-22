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
import { OrientationInput } from 'sensor-input-lib';
import { boundClass } from 'autobind-decorator';
import { Image, Layer, Stage, Text } from 'react-konva';
import {
  ControllerClient,
  Button,
  PlayerColor,
  PlayerInfo,
} from 'controller-client-lib';
import { ControlsHeaderRow } from './ControlsHeaderRow';
import { InfoBar } from './InfoBar';
import * as consts from './consts';

@boundClass
export class OrientationControls extends React.Component<
  OrientationControlsProbs,
  OrientationControlsState
> {
  private orientationInput: OrientationInput = new OrientationInput();

  constructor(probs: OrientationControlsProbs) {
    super(probs);

    this.orientationInput.onInputChange = this.onInputChanged;

    this.state = {
      vertical: 0,
      horizontal: 0,
      viewportWidth: document.documentElement.clientWidth,
      viewportHeight: document.documentElement.clientHeight,
    };
  }

  onInputChanged(vertical: number, horizontal: number) {
    this.props.client.setImuOrientation(vertical, horizontal);

    this.setState({
      vertical: vertical,
      horizontal: horizontal,
    });
  }

  onResize() {
    this.setState({
      viewportHeight: document.documentElement.clientHeight,
      viewportWidth: document.documentElement.clientWidth,
    });
  }

  componentDidMount() {
    window.onresize = this.onResize;
    this.orientationInput.start();
  }

  componentWillUnmount() {
    window.onresize = () => {};
    this.orientationInput.stop();
  }

  render() {
    const maybePlayerColorRaw = this.props.client.getColor();
    let playerColorRaw = PlayerColor.Any;
    if (maybePlayerColorRaw != null) {
      playerColorRaw = maybePlayerColorRaw;
    }

    const image = new window.Image();
    image.src = consts.platforms[playerColorRaw];

    const controlsBarHeightFactor = 0.12;
    const exitButtonHeightFactor = this.props.canExit ? 0.1 : 0.0;
    const canvasHeightFactor =
      1.0 - controlsBarHeightFactor - exitButtonHeightFactor;

    const canvasWidth = this.state.viewportWidth;
    const canvasHeight = canvasHeightFactor * this.state.viewportHeight;

    const platformWidthFactor = 0.2;
    const platformWidth = canvasWidth * platformWidthFactor;
    const platformRatio = image.height / image.width;
    const platformHeight = platformRatio * platformWidth;

    const platformXRange = canvasWidth - platformWidth;
    const platformYRange = canvasHeight - platformHeight;
    const platformXOffset = 0;
    const platformYOffset = 0;

    const platformX =
      platformXOffset +
      platformXRange / 2 +
      (this.state.horizontal * platformXRange) / 2;
    const platformY =
      platformYOffset +
      platformYRange / 2 +
      (-this.state.vertical * platformYRange) / 2;

    return (
      <div className='container' style={{ userSelect: 'none' }}>
        <InfoBar playerInfo={this.props.playerInfo} />
        <div
          className='rowContainer'
          style={{
            width: '50%',
            height: `${controlsBarHeightFactor * 100}%`,
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
        <div
          className='rowContainer'
          style={{ height: `${canvasHeightFactor * 100}%` }}
        >
          <div
            style={{
              position: 'absolute',
              left: '0px',
              bottom: `${exitButtonHeightFactor * 100}%`,
            }}
          >
            <Stage width={canvasWidth} height={canvasHeight}>
              <Layer>
                <Text
                  text='Tilt your phone to control the platform!'
                  fontFamily='m6x11'
                  fontSize={0.07 * canvasHeight}
                  fill='white'
                  width={canvasWidth}
                  align='center'
                  y={0.1 * canvasHeight}
                />
              </Layer>
              <Layer>
                <Image
                  image={image}
                  width={platformWidth}
                  height={platformHeight}
                  x={platformX}
                  y={platformY}
                />
              </Layer>
            </Stage>
          </div>
          {this.props.canExit && (
            <button
              className='controllerMenuItem text'
              style={{
                backgroundColor: consts.buttonStyles[Button.Exit].dark,
                borderColor: consts.buttonStyles[Button.Exit].light,
                position: 'absolute',
                left: '0px',
                bottom: '0px',
                height: `${exitButtonHeightFactor * 100}%`,
                width: '100%',
              }}
              onMouseDown={_ => this.props.client.setButton(Button.Exit, true)}
              onMouseUp={_ => this.props.client.setButton(Button.Exit, false)}
              onTouchStart={_ => this.props.client.setButton(Button.Exit, true)}
              onTouchEnd={_ => this.props.client.setButton(Button.Exit, false)}
              onTouchCancel={_ =>
                this.props.client.setButton(Button.Exit, false)
              }
            >
              Exit Platform
            </button>
          )}
        </div>
      </div>
    );
  }
}

interface OrientationControlsState {
  vertical: number;
  horizontal: number;
  viewportHeight: number;
  viewportWidth: number;
}

interface OrientationControlsProbs {
  client: ControllerClient;
  isFullscreen: boolean;
  toggleFullscreen: () => Promise<any>;
  playerColor: consts.ColorData;
  enabledButtons: Set<Button>;
  canPause: boolean;
  pause: () => void;
  canExit: boolean;
  canShowMap: boolean;
  showMap: () => void;
  playerInfo?: PlayerInfo;
}
