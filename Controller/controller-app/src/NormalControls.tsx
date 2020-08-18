import { Button, ControllerClient, PlayerInfo } from 'controller-client-lib';
import React from 'react';
import nipplejs, {
  JoystickManager,
  JoystickManagerOptions,
  EventData,
  JoystickOutputData,
} from 'nipplejs';
import { BarLoader as CooldownSpinner } from 'react-spinners';
import './Controller.css';

import * as consts from './consts';

import { boundClass } from 'autobind-decorator';
import { ControlsHeaderRow } from './ControlsHeaderRow';
import { InfoBar } from './InfoBar';

@boundClass
export class NormalControls extends React.Component<
  NormalControlsProbs,
  NormalControlsState
> {
  private boob: React.RefObject<HTMLDivElement>;

  constructor(probs: NormalControlsProbs) {
    super(probs);

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

    // Define button with colors and events for readability
    const buildButton = (button: Button) => {
      const info: consts.ColorData = consts.buttonStyles[button];

      const buttonElement = (
        <button
          key={button}
          className='pixelButton text'
          style={{ backgroundColor: info.dark, borderColor: info.light }}
          onMouseDown={_ => this.onButtonChanged(button, true)}
          onMouseUp={_ => this.onButtonChanged(button, false)}
          onTouchStart={_ => this.onButtonChanged(button, true)}
          onTouchEnd={_ => this.onButtonChanged(button, false)}
          onTouchCancel={_ => this.onButtonChanged(button, false)}
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
    };

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
      .map(buildButton);
    const enabledPrimaryButtons = enabledButtons
      .filter(button => consts.primaryButtons.has(button))
      .map(buildButton);
    const enabledSecondaryButtons = enabledButtons
      .filter(
        button =>
          !consts.itemButtons.has(button) && !consts.primaryButtons.has(button)
      )
      .map(buildButton);
    ensurePlaceholder(enabledItemButtons);
    ensurePlaceholder(enabledPrimaryButtons);
    ensurePlaceholder(enabledSecondaryButtons);

    // Return DOM elements
    return (
      <div className='container'>
        <div className='rowContainer' style={{ userSelect: 'none' }}>
          <div className='columnContainer'>
            <div className='gameControls'>
              <InfoBar playerInfo={this.props.playerInfo} />
              <div id='boob' className='text' ref={this.boob}>
                <p id='boobText'> tap and drag to move </p>
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
                    canExit={false}
                  />
                </div>
                <div className='buttonColumn'>
                  <div className='buttonRow'>{enabledItemButtons}</div>
                  <div className='buttonRow'>{enabledPrimaryButtons}</div>
                  <div className='buttonRow'>{enabledSecondaryButtons}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }
}

interface NormalControlsState {}

interface NormalControlsProbs {
  client: ControllerClient;
  isFullscreen: boolean;
  enabledButtons: Set<Button>;
  cooldownButtons: Set<Button>;
  toggleFullscreen: () => Promise<any>;
  playerColor: consts.ColorData;
  canPause: boolean;
  pause: () => void;
  playerInfo?: PlayerInfo;
}
