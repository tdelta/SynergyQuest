import WebSocket from 'isomorphic-ws';

import {JoystickMessage, ButtonMessage} from './Message';

/**
 * Stores the ids of the different controller buttons
 */
export enum Button {
  Attack = 0,
  Pull = 1
}

/**
 * Allows to connect to a Coop-Dungeon game and send controller inputs.
 * Instantiate using the static `create()` method.
 *
 * Internally, this class sends JSON encoded messages over websockets.
 * For the format of the websockets, see the `Message` class hierarchy.
 *
 * Usage example:
 * ```typescript
 *   import { ControllerInputSender, Button } from controller_input_sender;
 *
 *   ControllerInputSender
 *     .create('127.0.0.1')
 *     .then(sender => {
 *       sender.setButton(Button.Attack, true);
 *     });
 * ```
 *
 * For further examples, see the few tests in the folder `__tests__`.
 */
export class ControllerInputSender {
  private socket: WebSocket.WebSocket;

  /**
   * Connects to the game and creates a ControllerInputSender instance which is
   * returned inside a promise. As soon as the connection succeeded, the promise
   * is resolved.
   *
   * @param address The network address where the game is running
   * @param port Port where the game is listening for controller connections (default: 4242)
   **/
  public static connect(address: string, port: number = 4242): Promise<ControllerInputSender> {
    let socket = new WebSocket(`ws://${address}:${port}/sockets/`);

    // Create a promise which is resolved, as soon as the socket has been
    // successfully opened
    const connectionPromise = new Promise(resolve => 
        socket.onopen = (_: any) => {
          socket.onopen = (_: any) => {};
          resolve();
        }
    );

    return connectionPromise.then(
      () => new ControllerInputSender(socket)
    );
  }

  private constructor(socket: WebSocket) {
    this.socket = socket;
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
    let msg = new JoystickMessage(vertical, horizontal);

    if (vertical < -1 || vertical > 1 || horizontal < -1 || horizontal > 1) {
      throw Error("Only joystick positions in the interval [-1; 1] are allowed.");
    }

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
  setButton(button: Button, onOff: boolean) {
    let msg = new ButtonMessage(button, onOff);

    this.socket.send(
      JSON.stringify(msg)
    );
  }
}
