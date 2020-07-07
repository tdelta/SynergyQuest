import { Button, ControllerClient } from 'controller-client-lib';
import React from 'react';
import nipplejs, {
  JoystickManager,
  JoystickManagerOptions,
  EventData,
  JoystickOutputData,
} from 'nipplejs';
import './Controller.css';
import { ColorData } from './consts';
import * as consts from './consts';

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
  }

  onButtonChanged(button: Button, pressed: boolean) {
    this.props.client.setButton(button, pressed);
  }

  onJoystickMoved(_evt: EventData, data: JoystickOutputData) {
    const threshold: number = 0.2;

    let horizontal: number;
    let vertical: number;
    if (data.force > threshold) {
      const angle: number = data.angle.radian;

      horizontal = Math.cos(angle);
      vertical = Math.sin(angle);
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
    document.documentElement.requestFullscreen().catch(err => {
      console.error(
        `Error when trying to go full screen: ${err.message} (${err.name})`
      );
    });
  }

  componentWillUnmount() {
    if (document.fullscreenElement) {
      document.exitFullscreen();
    }
  }

  render() {
    // Define button with colors and events for readability
    const button = (
      text: string,
      button: Button,
      col1: string,
      col2: string
    ) => (
      <button
        className='pixelButton text'
        style={{ backgroundColor: col1, borderColor: col2 }}
        onMouseDown={_ => this.onButtonChanged(button, true)}
        onMouseUp={_ => this.onButtonChanged(button, false)}
        onTouchStart={_ => this.onButtonChanged(button, true)}
        onTouchEnd={_ => this.onButtonChanged(button, false)}
      >
        {text}
      </button>
    );

    let playerColor: ColorData = this.props.playerColor;
    
    let buttons: JSX.Element[] = [];

    buttons.push(button(
        consts.buttonStyles[Button.Attack].name,
        Button.Attack,
        consts.buttonStyles[Button.Attack].dark,
        consts.buttonStyles[Button.Attack].dark));
    console.log(this.props.enabledGameActions);
    this.props.enabledGameActions.forEach( (action: Button) =>
    {
        let info: ColorData = consts.buttonStyles[action];
        buttons.push(button(info.name, action, info.light, info.dark));
    });

    // Return DOM elements
    return (
      <div className='container'>
        <div className='rowContainer' style={{ userSelect: 'none' }}>
          <div className='columnContainer'>
            <div className='gameControls'>
              <div id='boob' className='text' ref={this.boob}>
                <p id='boobText'> tap and drag to move </p>
              </div>
              <div className='buttonColumn'>
                <div className='rowContainer'>
                  <button
                      className='colorIndicator text no-click'
                      style={{ backgroundColor: playerColor.dark, borderColor: playerColor.light }} >
                      {playerColor.name}
                  </button>
                </div>
                {buttons}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }
}

interface ControllerState {}

interface ControllerProbs {
  client: ControllerClient;
  playerColor: ColorData;
  enabledGameActions: Set<Button>;
}
