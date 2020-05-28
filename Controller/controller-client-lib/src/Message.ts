import { Button } from './ControllerClient';

export namespace MessageFormat {
  export enum MessageType {
    Button = 0,     // Sent by controller
    Joystick = 1,   // Sent by controller
    PlayerColor = 2 // Sent by the game
  }

  /**
   * This interface hierarchy determines the format of the JSON messages sent
   * between the game controller and the game.
   *
   * The base interface only carries an identifier which determines the type of the
   * message. See also the `MessageType` enum.
   */
  export interface Message {
    readonly type: MessageType
  }

  export function isMessage(obj: any): obj is Message {
    return 'type' in obj;
  }

  export interface ButtonMessage extends Message {
    readonly button: Button;
    readonly onOff: boolean;
  }

  export function createButtonMessage(button: Button, onOff: boolean): ButtonMessage {
    return {
      type: MessageType.Button,
      button: button,
      onOff: onOff
    };
  }

  export interface JoystickMessage extends Message {
    readonly vertical: number;
    readonly horizontal: number;
  }

  export function createJoystickMessage(vertical: number, horizontal: number): JoystickMessage {
    return {
      type: MessageType.Joystick,
      vertical: vertical,
      horizontal: horizontal
    };
  }

  export interface PlayerColorMessage extends Message {
    readonly color: string; // hexadecimal number with leading #, e.g. "#c0ffee"
  }

  export function isPlayerColorMessage(obj: any): obj is PlayerColorMessage {
    return isMessage(obj) && 'color' in obj;
  }

  export function messageFromJSON(str: string): Message {
    let msgObj = JSON.parse(str);

    if (!isMessage(msgObj)) {
      throw Error("Invalid message format.");
    }

    // TODO: We should do some more checks here, whether its a valid message, but
    // I wouldn't do this here until our protocol is finalized.
    // Until then its too much maintenance overhead.
    return msgObj;
  }
}
