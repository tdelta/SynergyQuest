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

import { IControllerClient, PlayerColor } from 'controller-client-lib';
import React, { createRef } from 'react';
import './Controller.css';
import PauseSymbol from './gfx/pause.png';
import MapSymbol from './gfx/map.png';
import ActivateFullscreenSymbol from './gfx/activate_fullscreen.png';
import DeactivateFullscreenSymbol from './gfx/deactivate_fullscreen.png';

import * as consts from './consts';

import { boundClass } from 'autobind-decorator';
import { SpeechBubble } from './SpeechBubble';

@boundClass
export class ControlsHeaderRow extends React.PureComponent<
  ControlsHeaderRowProps,
  ControlsHeaderRowState
> {
  private fullscreenHintRef = createRef<HTMLButtonElement>();
  private fullscreenHintTimeoutHandle: number | undefined;

  constructor(props: ControlsHeaderRowProps) {
    super(props);

    this.state = {
      showFullscreenHint: false,
      hintTimedOut: false,
    };
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
          <div
            className='middleBorders controllerMenuItem text'
            style={{
              backgroundColor: '#e0e0e0', // cplayerColor.dark,
              alignItems: 'center',
              backgroundImage: `url(${consts.avatars[playerColorRaw]})`,
              backgroundSize: 'auto 100%',
            }}
          />
          {this.props.canPause && (
            <button
              className='middleBorders controllerMenuItem text'
              style={{
                backgroundImage: `url(${PauseSymbol})`,
              }}
              onClick={_ => this.props.pause()}
            />
          )}
          {this.props.canShowMap && (
            <button
              className='middleBorders controllerMenuItem text'
              style={{
                backgroundImage: `url(${MapSymbol})`,
              }}
              onClick={this.props.showMap}
            />
          )}
          <button
            ref={this.fullscreenHintRef}
            className='rightBorders controllerMenuItem text'
            style={{
              backgroundImage: this.props.isFullscreen
                ? `url(${DeactivateFullscreenSymbol})`
                : `url(${ActivateFullscreenSymbol})`,
            }}
            onClick={this.props.toggleFullscreen}
          />
          {this.state.showFullscreenHint && !this.state.hintTimedOut && (
            <SpeechBubble
              style={{ fontSize: '6vh' }}
              target={this.fullscreenHintRef}
            >
              Press this to stop orientation changes and enable full-screen
            </SpeechBubble>
          )}
        </div>
      </>
    );
  }

  triggerFullscreenHint() {
    if (!this.props.isFullscreen) {
      if (!this.state.showFullscreenHint) {
        this.setHintVisibility(true);
      }
    } else if (this.state.showFullscreenHint) {
      this.setHintVisibility(false);
    }
  }

  componentDidMount() {
    window.addEventListener('orientationchange', this.onOrientationChange);

    this.triggerFullscreenHint();
  }

  componentWillUnmount() {
    window.removeEventListener('orientationchange', this.onOrientationChange);
  }

  componentDidUpdate(
    prevProps: Readonly<ControlsHeaderRowProps>,
    prevState: Readonly<ControlsHeaderRowState>,
    snapshot?: any
  ) {
    this.triggerFullscreenHint();
  }

  onOrientationChange() {
    this.setHintVisibility(true);
  }

  setHintVisibility(visible: boolean) {
    if (this.fullscreenHintTimeoutHandle != null) {
      window.clearTimeout(this.fullscreenHintTimeoutHandle);
      this.fullscreenHintTimeoutHandle = undefined;
    }

    this.setState({
      showFullscreenHint: visible,
      hintTimedOut: false,
    });

    this.fullscreenHintTimeoutHandle = window.setTimeout(() => {
      this.setState({
        hintTimedOut: true,
      });
    }, 5000);
  }
}

interface ControlsHeaderRowState {
  showFullscreenHint: boolean;
  hintTimedOut: boolean;
}

interface ControlsHeaderRowProps {
  client: IControllerClient;
  isFullscreen: boolean;
  toggleFullscreen: () => any;
  playerColor: consts.ColorData;
  canPause: boolean;
  pause: () => void;
  canShowMap: boolean;
  showMap: () => void;
}
