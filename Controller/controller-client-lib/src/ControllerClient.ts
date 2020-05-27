import WebSocket from 'isomorphic-ws';

import {MessageFormat} from './Message';

/**
 * Stores the ids of the different controller buttons
 */
export enum Button {
  Attack = 0,
  Pull = 1
}

/**
 * Allows to connect to a Coop-Dungeon game and send controller inputs
 * or receive messages from the game.
 * Instantiate using the static `create()` method.
 *
 * Internally, this class sends and receives JSON encoded messages over
 * websockets.  For the format of the websockets, see the `Message` class
 * hierarchy.
 *
 * Usage example:
 * ```typescript
 *   import { ControllerClient, Button } from controller_client_lib;
 *
 *   ControllerClient
 *     .connect('127.0.0.1')
 *     .then(sender => {
 *       sender.setButton(Button.Attack, true);
 *     });
 * ```
 *
 * For further examples, see the few tests in the folder `__tests__` or the
 * example application `controller-lib-test-app`.
 */
export class ControllerClient {
  private socket: WebSocket.WebSocket;

  // Player color assigned by the game. May be undefined until the server
  // sends a colour.
  private color?: string;

  /**
   * Callback which is called when a connection to a game has been established
   */
  private onConnect: () => any;

  /**
   * Callback which is called when the connection to a game has been closed
   */
  private onDisconnect: () => any;

  /**
   * Callback which is called when the connection to a game has been closed due
   * to an error.
   */
  private onError: () => any;

  /**
   * Callback which is called whenever the server assigns this client a color.
   */
  private onSetPlayerColor: (color: string) => any;

  /**
   * Connects to the game and creates a ControllerClient instance.
   * Be aware that the client is not immediately connected.
   *
   * Register callbacks `onConnect`, `onDisconnect`, `onError` to keep track of
   * when the client is ready for use.
   *
   * @param address          The network address where the game is running
   * @param onConnect        Callback which is called when a connection to a game has been established
   * @param onDisconnect     Callback which is called when the connection to a game has been closed (optional)
   * @param onError          Callback which is called when the connection to a game has been closed due to an error (optional)
   * @param onSetPlayerColor Callback which is called whenever the server assigns this client a color. (optional)
   * @param port             Port where the game is listening for controller connections (optional, default: 4242)
   **/
  public constructor(
    address: string,
    onConnect: () => any,
    onDisconnect: () => any = () => {},
    onError: () => any = () => {},
    onSetPlayerColor: (color: string) => any = (_: string) => {},
    port: number = 4242
  ) {
    this.onMessage = this.onMessage.bind(this);
    this.setJoystickPosition = this.setJoystickPosition.bind(this);
    this.setButton = this.setButton.bind(this);
    this.getColor = this.getColor.bind(this);

    this.onConnect = onConnect;
    this.onDisconnect = onDisconnect;
    this.onError = onError;
    this.onSetPlayerColor = onSetPlayerColor;

    this.socket = this.connect(address, port);

    this.socket.onopen = (_: Event) => this.onConnect();
    this.socket.onclose = (_: CloseEvent) => this.onDisconnect();
    this.socket.onerror = (_: Event) => {
      console.error("ControllerClient: Connection to game closed due to an error.");
      this.onError();
    };
    this.socket.onmessage = this.onMessage;
  }

  /**
   * Sends the position of the joystick.
   *
   * @param vertical    vertical position of the joystick. Must be a floating
   *                    point number in [-1; 1]
   * @param horizontal  horizontal position of the joystick. Must be a floating
   *                    point number in [-1; 1]
   */
  public setJoystickPosition(vertical: number, horizontal: number) {
    if (vertical < -1 || vertical > 1 || horizontal < -1 || horizontal > 1) {
      throw Error("Only joystick positions in the interval [-1; 1] are allowed.");
    }

    let msg = MessageFormat.createJoystickMessage(vertical, horizontal);

    this.socket.send(
      JSON.stringify(msg)
    );
  }

  /**
   * Sends, whether a button has been pressed or released.
   *
   * @param button identifier of the button, see `Button` enum
   * @param onOff  whether the button is pressed (`true`) or not (`false`)
   */
  public setButton(button: Button, onOff: boolean) {
    let msg = MessageFormat.createButtonMessage(button, onOff);

    this.socket.send(
      JSON.stringify(msg)
    );
  }

  /**
   * Returns the color assigned to this controller by the game.
   * May be undefined, if the game has not sent a color yet.
   *
   * To be notified when a color is assigned, set the callback
   * `onSetPlayerColor`.
   *
   * @returns hexadecimal color value with leading '#' or undefined. E.g. "#c0ffee".
   */
  public getColor(): string | undefined {
    return this.color;
  }

  public isConnected(): boolean {
    return this.socket.readyState === 1;
  }

  /**
   * Connects to the game and creates a websocket.
   *
   * @param address The network address where the game is running
   * @param port Port where the game is listening for controller connections (default: 4242)
   **/
  private connect(address: string, port: number = 4242): WebSocket {
    let socket = new WebSocket(`ws://${address}:${port}/sockets/`);

    return socket;
  }

  /**
   * Handles messages sent by the server
   *
   * @param msgEvent message event received over the websocket, see also `WebSocket.onmessage`.
   */
  private onMessage(msgEvent: MessageEvent) {
    // deserialize the message from JSON
    let msg = MessageFormat.messageFromJSON(msgEvent.data);

    // If the server sent a player color, set the color and call the callback
    // `onSetPlayerColor` which can be set by users of the library.
    if (MessageFormat.isPlayerColorMessage(msg)) {
      this.color = msg.color;
      this.onSetPlayerColor(msg.color);
    }
  }
}
