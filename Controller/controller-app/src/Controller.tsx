import { Button, MenuAction, ControllerClient, ConnectFailureReason } from 'controller-client-lib';
import React from 'react';
import nipplejs, { JoystickManager, JoystickManagerOptions, EventData, JoystickOutputData } from 'nipplejs';
import './Controller.css';


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
    document.documentElement.requestFullscreen().catch(err => {
      console.error(`Error when trying to go full screen: ${err.message} (${err.name})`);
    });
  }

  componentWillUnmount() {
    if (document.fullscreenElement) {
      document.exitFullscreen();
    }
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
      <div className='rowContainer' style={{userSelect: 'none'}}>
        <div id='boob' className='text' ref={this.boob}> 
          <p id='boobText'> tap and drag to move </p>
        </div>
        <div className='columnContainer'>
          {button('Attack', Button.Attack, '#E53935', '#EF5350')}
          {button('Pull', Button.Pull, '#039BE5', '#29B6F6')}
        </div>
      </div>
      </div>
    );
  }
}

interface ControllerState {
}

interface ControllerProbs {
  client: ControllerClient;
}
