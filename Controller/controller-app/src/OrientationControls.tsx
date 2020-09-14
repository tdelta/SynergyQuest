import React from 'react';
import './Controller.css';
import { OrientationInput } from 'sensor-input-lib';
import { boundClass } from 'autobind-decorator';
import { Image, Layer, Stage, Text } from 'react-konva';
import {
  ControllerClient,
  Button,
  PlayerColor,
  PlayerInfo,
} from 'controller-client-lib';
import { ControlsHeaderRow } from './ControlsHeaderRow';
import { InfoBar } from './InfoBar';
import * as consts from './consts';

@boundClass
export class OrientationControls extends React.Component<
  OrientationControlsProbs,
  OrientationControlsState
> {
  private orientationInput: OrientationInput = new OrientationInput();

  constructor(probs: OrientationControlsProbs) {
    super(probs);

    this.orientationInput.onInputChange = this.onInputChanged;

    this.state = {
      vertical: 0,
      horizontal: 0,
      viewportWidth: document.documentElement.clientWidth,
      viewportHeight: document.documentElement.clientHeight,
    };
  }

  onInputChanged(vertical: number, horizontal: number) {
    this.props.client.setImuOrientation(vertical, horizontal);

    this.setState({
      vertical: vertical,
      horizontal: horizontal,
    });
  }

  onResize() {
    this.setState({
      viewportHeight: document.documentElement.clientHeight,
      viewportWidth: document.documentElement.clientWidth,
    });
  }

  componentDidMount() {
    window.onresize = this.onResize;
    this.orientationInput.start();
  }

  componentWillUnmount() {
    window.onresize = () => {};
    this.orientationInput.stop();
  }

  render() {
    const maybePlayerColorRaw = this.props.client.getColor();
    let playerColorRaw = PlayerColor.Any;
    if (maybePlayerColorRaw != null) {
      playerColorRaw = maybePlayerColorRaw;
    }

    const image = new window.Image();
    image.src = consts.platforms[playerColorRaw];

    const canvasWidth = this.state.viewportWidth;
    const canvasHeight = 0.88 * this.state.viewportHeight;

    const platformWidthFactor = 0.2;
    const platformWidth = canvasWidth * platformWidthFactor;
    const platformRatio = image.height / image.width;
    const platformHeight = platformRatio * platformWidth;

    const platformXRange = canvasWidth - platformWidth;
    const platformYRange = canvasHeight - platformHeight;
    const platformXOffset = 0;
    const platformYOffset = 0;

    const platformX =
      platformXOffset +
      platformXRange / 2 +
      (this.state.horizontal * platformXRange) / 2;
    const platformY =
      platformYOffset +
      platformYRange / 2 +
      (-this.state.vertical * platformYRange) / 2;

    return (
      <div className='container' style={{ userSelect: 'none' }}>
        <InfoBar playerInfo={this.props.playerInfo} />
        <div
          className='rowContainer'
          style={{
            width: '50%',
            height: '12%',
            left: '50%',
            top: '0',
            position: 'absolute',
          }}
        >
          <ControlsHeaderRow
            client={this.props.client}
            isFullscreen={this.props.isFullscreen}
            toggleFullscreen={this.props.toggleFullscreen}
            playerColor={this.props.playerColor}
            canPause={this.props.canPause}
            pause={this.props.pause}
            canShowMap={this.props.canShowMap}
            showMap={this.props.showMap}
            canExit={this.props.enabledButtons.has(Button.Exit)}
          />
        </div>
        <div className='rowContainer' style={{ height: '88%' }}>
          <Stage width={canvasWidth} height={canvasHeight}>
            <Layer>
              <Text
                text='Tilt your phone to control the platform!'
                fontFamily='m6x11'
                fontSize={0.07 * canvasHeight}
                fill='white'
                width={canvasWidth}
                align='center'
                y={0.1 * canvasHeight}
              />
            </Layer>
            <Layer>
              <Image
                image={image}
                width={platformWidth}
                height={platformHeight}
                x={platformX}
                y={platformY}
              />
            </Layer>
          </Stage>
        </div>
      </div>
    );
  }
}

interface OrientationControlsState {
  vertical: number;
  horizontal: number;
  viewportHeight: number;
  viewportWidth: number;
}

interface OrientationControlsProbs {
  client: ControllerClient;
  isFullscreen: boolean;
  toggleFullscreen: () => Promise<any>;
  playerColor: consts.ColorData;
  enabledButtons: Set<Button>;
  canPause: boolean;
  pause: () => void;
  canShowMap: boolean;
  showMap: () => void;
  playerInfo?: PlayerInfo;
}
