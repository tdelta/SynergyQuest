import {
  Button,
  ControllerClient,
  InputMode,
  PlayerInfo,
} from 'controller-client-lib';
import React from 'react';
import './Controller.css';
import fscreen from 'fscreen';

import * as consts from './consts';
import { boundClass } from 'autobind-decorator';
import { NormalControls } from './NormalControls';
import { OrientationControls } from './OrientationControls';

@boundClass
export class Controller extends React.Component<
  ControllerProbs,
  ControllerState
> {
  private orientationIsLocked: boolean = false;

  constructor(probs: ControllerProbs) {
    super(probs);

    this.state = {
      isFullscreen: false,
    };
  }

  async toggleFullscreen() {
    const makeFullscreen = !this.state.isFullscreen;

    // Hacky solution: Type should be Promise<void> but sadly the type definitions of fscreen say something else ...
    let fullscreenFn: any;

    if (makeFullscreen) {
      fullscreenFn = () =>
        fscreen.requestFullscreen(document.documentElement) as any;
    } else {
      fullscreenFn = fscreen.exitFullscreen as any;
    }

    try {
      await fullscreenFn();

      if (makeFullscreen) {
        // Lock orientation to landscape if possible
        try {
          await window.screen.orientation.lock('landscape-primary');
          this.orientationIsLocked = true;
        } catch (error) {
          this.orientationIsLocked = false;
          console.error(`Failed to lock screen to landscape: ${error}`);
        }
      }
    } catch (error) {
      console.error(
        `Error while requesting fullscreen: ${error.name} -- ${error.message}`
      );
    }

    this.setState({ isFullscreen: makeFullscreen });
  }

  onFullscreenChange() {
    this.setState({
      // Evaluates to true if window is in fullscreen
      isFullscreen: fscreen.fullscreenElement !== null,
    });
  }

  async componentDidMount() {
    // The titlebar is annoying on mobile, so we get rid of it
    await this.toggleFullscreen();
    fscreen.onfullscreenchange = this.onFullscreenChange;
  }

  componentWillUnmount() {
    // Unlock screen orientation, if it was locked before
    if (this.orientationIsLocked) {
      this.orientationIsLocked = false;
      window.screen.orientation.unlock();
    }
  }

  render() {
    if (this.props.inputMode === InputMode.IMUOrientation) {
      return (
        <OrientationControls
          client={this.props.client}
          isFullscreen={this.state.isFullscreen}
          toggleFullscreen={this.toggleFullscreen}
          playerColor={this.props.playerColor}
          canPause={this.props.canPause}
          pause={this.props.pause}
          enabledButtons={this.props.enabledButtons}
          playerInfo={this.props.playerInfo}
        />
      );
    } else {
      return (
        <NormalControls
          client={this.props.client}
          isFullscreen={this.state.isFullscreen}
          enabledButtons={this.props.enabledButtons}
          cooldownButtons={this.props.cooldownButtons}
          toggleFullscreen={this.toggleFullscreen}
          playerColor={this.props.playerColor}
          canPause={this.props.canPause}
          pause={this.props.pause}
          playerInfo={this.props.playerInfo}
        />
      );
    }
  }
}

interface ControllerState {
  isFullscreen: boolean;
}

interface ControllerProbs {
  client: ControllerClient;
  inputMode: InputMode;
  playerColor: consts.ColorData;
  enabledButtons: Set<Button>;
  cooldownButtons: Set<Button>;
  canPause: boolean;
  pause: () => void;
  displayFailure: (message: string) => void;
  playerInfo?: PlayerInfo;
}
