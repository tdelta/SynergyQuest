import { Button, ControllerClient, PlayerColor } from 'controller-client-lib';
import React from 'react';
import nipplejs, {
  JoystickManager,
  JoystickManagerOptions,
  EventData,
  JoystickOutputData,
} from 'nipplejs';
import './Controller.css';
import fscreen from 'fscreen';
import PauseSymbol from './gfx/pause.png';

import * as consts from './consts';

import { faCompress, faExpand } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';

export class Controller extends React.Component<
  ControllerProbs,
  ControllerState
> {
  private boob: React.RefObject<HTMLDivElement>;
  private client: ControllerClient;

  constructor(probs: ControllerProbs) {
    super(probs);

    this.boob = React.createRef();

    this.client = probs.client;

    this.onJoystickMoved = this.onJoystickMoved.bind(this);
    this.onJoystickReleased = this.onJoystickReleased.bind(this);
    this.onButtonChanged = this.onButtonChanged.bind(this);
    this.toggleFullscreen = this.toggleFullscreen.bind(this);
    this.onFullscreenChange = this.onFullscreenChange.bind(this);

    this.state = {
      isFullscreen: false,
    };
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

    this.client.setJoystickPosition(vertical, horizontal);
  }

  onJoystickReleased(_evt: EventData, _data: JoystickOutputData) {
    const horizontal: number = 0;
    const vertical: number = 0;
    this.client.setJoystickPosition(vertical, horizontal);
  }

  toggleFullscreen() {
    // Hacky solution: Type should be Promise<void> but sadly the type definitions of fscreen say something else ...
    let fullscreenFn: any;

    if (!this.state.isFullscreen) {
      fullscreenFn = fscreen.requestFullscreenFunction(
        document.documentElement
      ) as any;
    } else {
      fullscreenFn = fscreen.exitFullscreen as any;
    }

    fullscreenFn
      .call(document.documentElement)
      .then(
        // Only set the state, if the fullscreen request was successfull
        () => this.setState({ isFullscreen: !this.state.isFullscreen })
      )
      .catch((error: Error) =>
        console.error(
          `Error while requesting fullscreen: ${error.name} -- ${error.message}`
        )
      );
  }

  onFullscreenChange() {
    this.setState({
      // Evaluates to true if window is in fullscreen
      isFullscreen: fscreen.fullscreenElement !== null,
    });
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

    // The titlebar is annoying on mobile, so we get rid of it
    this.toggleFullscreen();
    fscreen.onfullscreenchange = this.onFullscreenChange;
  }

  render() {
    // Define button with colors and events for readability
    const buildButton = (button: Button) => {
      const info: consts.ColorData = consts.buttonStyles[button];

      return (
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
    };

    const maybePlayerColorRaw = this.props.client.getColor();
    let playerColorRaw = PlayerColor.Any;
    if (maybePlayerColorRaw != null) {
      playerColorRaw = maybePlayerColorRaw;
    }

    const enabledButtons = Array.from(this.props.enabledButtons);

    const placeholderButton = (
      <button
        className='pixelButton text'
        style={{ backgroundColor: 'grey', borderColor: 'white' }}
      ></button>
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
              <div id='boob' className='text' ref={this.boob}>
                <p id='boobText'> tap and drag to move </p>
              </div>
              <div className='rightColumn'>
                <div className='rowContainer' style={{ height: '12%' }}>
                  <div className='controllerMenuContainer'>
                    <button
                      className='controllerMenuItem text no-click'
                      style={{
                        backgroundColor: '#e0e0e0', // cplayerColor.dark,
                        borderColor: '#eeeeee',
                        alignItems: 'center',
                      }}
                    >
                      <img
                        className='playerAvatar'
                        alt='Player Avatar'
                        src={consts.avatars[playerColorRaw]}
                      />
                    </button>
                    {this.props.canPause && (
                      <button
                        className='controllerMenuItem text'
                        style={{
                          backgroundColor: '#c2185b',
                          borderColor: '#e91e63',
                        }}
                        onClick={_ => this.props.pause()}
                      >
                        <img
                          className='pauseSymbol'
                          alt='Pause Game'
                          src={PauseSymbol}
                        />
                      </button>
                    )}
                  </div>
                  <div id='fullscreenButton' onClick={this.toggleFullscreen}>
                    <FontAwesomeIcon
                      icon={this.state.isFullscreen ? faCompress : faExpand}
                    />
                  </div>
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

interface ControllerState {
  isFullscreen: boolean;
}

interface ControllerProbs {
  client: ControllerClient;
  playerColor: consts.ColorData;
  enabledButtons: Set<Button>;
  canPause: boolean;
  pause: () => void;
  displayFailure: (message: string) => void;
}
