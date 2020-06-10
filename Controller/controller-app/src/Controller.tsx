import { Button, MenuAction, ControllerClient, ConnectFailureReason } from 'controller-client-lib';
import React from 'react';
import nipplejs, { JoystickManager, JoystickManagerOptions, EventData, JoystickOutputData } from 'nipplejs';
import './Controller.css';


export class Controller extends React.Component<ControllerProbs, {}> {

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

  onButtonChanged(e: React.MouseEvent, pressed: boolean) {
    let button: Button = Button.Attack;
    let target: HTMLButtonElement = e.target as HTMLButtonElement;
    switch(target.id) {
      case 'attack':
        button = Button.Attack;
        break;
      case 'pull':
        button = Button.Pull;
        break;
    }

    this.props.client.setButton(button, pressed);
  }

  onJoystickMoved(evt: EventData, data: JoystickOutputData) {
    console.log(evt.type);
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

  onJoystickReleased(evt: EventData, data: JoystickOutputData) {
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
  }

  render() {
    // Define button with colors and events for readability
    let button = (text: string, id: string, col1: string, col2: string) =>
    <button className='pixelButton text' id={id} style={{'backgroundColor': col1, 'borderColor': col2}} onMouseDown={e => this.onButtonChanged(e, true)} onMouseUp={e => this.onButtonChanged(e, false)}> {text} </button>

    // Return DOM elements
    return(
      <div className='container'>
      <div className='rowContainer'>
        <div id='boob' className='text' ref={this.boob}> 
          <p id='boobText'> tap and drag to move </p>
        </div>
        <div className='columnContainer'>
          {button('Attack','attack','#E53935','#EF5350')}
          {button('Pull','pull','#039BE5', '#29B6F6')}
        </div>
      </div>
      </div>
    );
  }
}

interface ControllerProbs {
  client: ControllerClient;
}
