import React from 'react';
import { boundClass } from 'autobind-decorator';
import { OrientationInput } from './OrientationInput';

export interface AppState {
  horizontal: number;
  vertical: number;
  message: string;
}

/**
 * Main UI class
 */
@boundClass
class App extends React.Component<{}, AppState> {
  private orientationInput: OrientationInput = new OrientationInput();

  private static readonly initialState: AppState = {
    vertical: 0,
    horizontal: 0,
    message: 'INIT',
  };

  constructor(props: {}) {
    super(props);

    this.orientationInput.onWarning = this.handleError;
    this.orientationInput.onFatalError = this.handleError;
    this.orientationInput.onInputChange = this.handleInputChange;
    this.orientationInput.onActivated = () => {
      this.setState({
        message: 'READY'
      })
    }

    // Initialize as not connected
    this.state = App.initialState;
  }

  handleInputChange(vertical: number, horizontal: number) {
    this.setState({
      vertical: vertical,
      horizontal: horizontal
    })
  }

  handleError(message: string) {
    console.error(message);

    this.setState({
      message: message
    })
  }

  componentDidMount() {
    this.setState({
      message: 'MOUNTING',
    });

    this.orientationInput.start();
  }

  componentWillUnmount() {
    this.orientationInput.stop();
  }

  render() {
    const angles = this.orientationInput.getRawAngle();
    const quaternion = this.orientationInput.getRawQuaternion();

    return <div>
      Yaw: {angles.yaw.toFixed(2)}<br/>
      Pitch: {angles.pitch.toFixed(2)}<br/>
      Roll: {angles.roll.toFixed(2)}<br/>
      Quaternion: [{quaternion[0].toFixed(2)}] [{quaternion[1].toFixed(2)} [{quaternion[2].toFixed(2)}] [{quaternion[3].toFixed(2)}]<br/>
      Vertical: {this.state.vertical}<br/>
      Horizontal: {this.state.horizontal}<br/>
      Message: {this.state.message}
    </div>;
  }
}

export default App;
