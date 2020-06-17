import { Button, MenuAction, ControllerClient, ConnectFailureReason } from 'controller-client-lib';
import React from 'react';
import nipplejs, { JoystickManager, JoystickManagerOptions, EventData, JoystickOutputData } from 'nipplejs';
import './Controller.css';
import { ColorData } from './consts';
import fscreen from 'fscreen';

import { faCompress, faExpand } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export class Controller extends React.Component<ControllerProbs, ControllerState> {

  private boob: React.RefObject<HTMLDivElement>;
  private client: ControllerClient;
  
  constructor(probs: ControllerProbs){
    super(probs);

    this.boob = React.createRef();
    
    this.client = probs.client;

    this.onJoystickMoved = this.onJoystickMoved.bind(this);
    this.onJoystickReleased = this.onJoystickReleased.bind(this);
    this.onButtonChanged = this.onButtonChanged.bind(this);
    this.toggleFullscreen = this.toggleFullscreen.bind(this);
    this.onFullscreenChange = this.onFullscreenChange.bind(this);

    this.state = {
      isFullscreen: false
    };
  }

  onButtonChanged(button: Button, pressed: boolean) {
    this.props.client.setButton(button, pressed);
  }

  onJoystickMoved(_evt: EventData, data: JoystickOutputData) {
    var threshold: number = 0.2;

    if (data.force > threshold) {
      var angle: number = data.angle.radian;
      var horizontal: number = Math.cos(angle);
      var vertical: number = Math.sin(angle);
    } else {
      var horizontal: number = 0;
      var vertical: number = 0;
    }

    this.client.setJoystickPosition(vertical, horizontal);
  }

  onJoystickReleased(_evt: EventData, _data: JoystickOutputData) {
    var horizontal: number = 0;
    var vertical: number = 0;
    this.client.setJoystickPosition(vertical, horizontal);
  }

  toggleFullscreen() {
    // Hacky solution: Type should be Promise<void> but sadly the type definitions of fscreen say something else ...
    let fullscreenFn: any = undefined;
    
    if (! this.state.isFullscreen){
        fullscreenFn = fscreen.requestFullscreenFunction(document.documentElement) as any;
    } else {
        fullscreenFn = fscreen.exitFullscreen as any;
    }
    fullscreenFn.call(document.documentElement)
    .then(
      // Only set the state, if the fullscreen request was successfull
      () => this.setState({ isFullscreen: !this.state.isFullscreen })
    )
    .catch(
      (error: Error) => console.error('Error while requesting fullscreen')
    );
  }

  onFullscreenChange() {
    this.setState({
      // Evaluates to true if window is in fullscreen
      isFullscreen: fscreen.fullscreenElement !== null
    });
  }

  componentDidMount() {
    // Create joystick on div "boob"
    var boob = this.boob.current!; // Boob definetly exists when componentDidMount is called
    var options: JoystickManagerOptions = {
      zone: boob,
    }
    var manager: JoystickManager = nipplejs.create(options);
    manager.on('move',this.onJoystickMoved);
    manager.on('end',this.onJoystickReleased);

    // The titlebar is annoying on mobile, so we get rid of it
    this.toggleFullscreen();
    fscreen.onfullscreenchange = this.onFullscreenChange;
  }

  render() {
    // Define button with colors and events for readability
    let button = (text: string, button: Button, col1: string, col2: string) =>
      <button
        className='pixelButton text'
        style={{'backgroundColor': col1, 'borderColor': col2}}
        onMouseDown={_ => this.onButtonChanged(button, true)}
        onMouseUp={_ => this.onButtonChanged(button, false)}
        onTouchStart={_ => this.onButtonChanged(button, true)}
        onTouchEnd={_ => this.onButtonChanged(button, false)}
      >
        {text}
      </button>

    // Return DOM elements
    return(
      <div className='container'>
        <div className='rowContainer' style={{ userSelect: 'none' }}>
          <div className='columnContainer'>
            <div className='gameControls'>
              <div id='boob' className='text' ref={this.boob}>
                <p id='boobText'> tap and drag to move </p>
              </div>
              <div className='buttonColumn'>
                <div className='rowContainer' style={{height:'10%'}}>
                  <button
                      className='colorIndicator text no-click'
                      style={{ backgroundColor: playerColor.dark, borderColor: playerColor.light }} >
                      {playerColor.name}
                  </button>
                  <div id='fullscreenButton' onClick={this.toggleFullscreen}>
                    <FontAwesomeIcon icon={this.state.isFullscreen ? faCompress : faExpand} />
                  </div>
                </div>
                {button('Attack', Button.Attack, '#E53935', '#EF5350')}
                {button('Pull', Button.Pull, '#039BE5', '#29B6F6')}
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
}
