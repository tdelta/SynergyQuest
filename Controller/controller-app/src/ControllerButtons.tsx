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

import { Button } from 'controller-client-lib';
import * as consts from './consts';

import { BarLoader as CooldownSpinner } from 'react-spinners';
import React from 'react';

import { boundClass } from 'autobind-decorator';

@boundClass
export class ControllerButtons extends React.PureComponent<
  ControllerButtonsProps,
  ControllerButtonsState
> {
  constructor(props: ControllerButtonsProps) {
    super(props);
    this.state = {};
  }

  /**
   * Render a single button with corresponding image/ text, color, events.
   * @param button - Button type (Attack, Pull etc)
   */
  renderButton(button: Button) {
    const buildCooldownOverlay = (inner: JSX.Element) => (
      <div className='cooldownWrapper'>
        {inner}
        <div className='cooldownOverlay'>
          <div className='cooldownProgressWrapper'>
            <CooldownSpinner
              css='background-color: rgba(255, 255, 255, 0.5)'
              width='100%'
              height='100%'
              color='rgba(255, 255, 255, 0.9)'
            />
          </div>
        </div>
      </div>
    );

    const info: consts.ColorData = consts.buttonStyles[button];

    const buttonElement = (
      <button
        key={button}
        className='pixelButton text'
        style={{ backgroundColor: info.dark, borderColor: info.light }}
        onMouseDown={_ => this.props.onButtonChanged(button, true)}
        onMouseUp={_ => this.props.onButtonChanged(button, false)}
        onTouchStart={_ => this.props.onButtonChanged(button, true)}
        onTouchEnd={_ => this.props.onButtonChanged(button, false)}
        onTouchCancel={_ => this.props.onButtonChanged(button, false)}
      >
        {info.image != null ? (
          <img alt='' className='buttonImage' src={info.image} />
        ) : (
          info.name
        )}
      </button>
    );

    if (this.props.cooldownButtons.has(button)) {
      return buildCooldownOverlay(buttonElement);
    } else {
      return buttonElement;
    }
  }

  render() {
    const enabledButtons = Array.from(this.props.enabledButtons);

    const placeholderButton = (
      <button
        className='pixelButton text'
        style={{ backgroundColor: '#e0e0e0', borderColor: '#f5f5f5' }}
      >
        &nbsp;
      </button>
    );
    const ensurePlaceholder = (buttons: Array<JSX.Element>) => {
      if (buttons.length === 0) {
        buttons.push(placeholderButton);
      }
    };
    const enabledItemButtons = enabledButtons
      .filter(button => consts.itemButtons.has(button))
      .map(this.renderButton);
    const enabledPrimaryButtons = enabledButtons
      .filter(button => consts.primaryButtons.has(button))
      .map(this.renderButton);
    const enabledSecondaryButtons = enabledButtons
      .filter(
        button =>
          !consts.itemButtons.has(button) && !consts.primaryButtons.has(button)
      )
      .map(this.renderButton);
    ensurePlaceholder(enabledItemButtons);
    ensurePlaceholder(enabledPrimaryButtons);
    ensurePlaceholder(enabledSecondaryButtons);

    return (
      <div className='buttonColumn'>
        <div className='buttonRow'>{enabledItemButtons}</div>
        <div className='buttonRow'>{enabledPrimaryButtons}</div>
        <div className='buttonRow'>{enabledSecondaryButtons}</div>
      </div>
    );
  }
}

interface ControllerButtonsProps {
  enabledButtons: Set<Button>;
  cooldownButtons: Set<Button>;
  onButtonChanged: (button: Button, pressed: boolean) => void;
}

interface ControllerButtonsState {}
