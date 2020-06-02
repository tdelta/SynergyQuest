import { Button } from './ControllerClient';

export namespace MessageFormat {
  export enum MessageType {
    Button = 0,            // Sent by controller
    Joystick = 1,          // Sent by controller
    PlayerColor = 2,       // Sent by the game
    Name = 3,              // Sent by controller
    NameTaken = 4,         // Sent by game
    NameOk = 5,            // Sent by game
    MaxPlayersReached = 6, // Sent by game
    None = 7               // Placeholder type for base class
  }

  export interface Matcher {
    readonly ButtonMessage:            (_: ButtonMessage) => any           ;
    readonly JoystickMessage:          (_: JoystickMessage) => any         ;
    readonly PlayerColorMessage:       (_: PlayerColorMessage) => any      ;
    readonly NameMessage:              (_: NameMessage) => any             ;
    readonly NameTakenMessage:         (_: NameTakenMessage) => any        ;
    readonly NameOkMessage:            (_: NameOkMessage) => any           ;
    readonly MaxPlayersReachedMessage: (_: MaxPlayersReachedMessage) => any;
  }

  export let defaultMatcher: Matcher = {
    ButtonMessage:            _ => {},
    JoystickMessage:          _ => {},
    PlayerColorMessage:       _ => {},
    NameMessage:              _ => {},
    NameTakenMessage:         _ => {},
    NameOkMessage:            _ => {},
    MaxPlayersReachedMessage: _ => {}
  };

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

  export function matchMessage(msg: Message, matcher: Matcher) {
    switch (msg.type) {
      case MessageType.Button:            matcher.ButtonMessage(msg as ButtonMessage); break;
      case MessageType.Joystick:          matcher.JoystickMessage(msg as JoystickMessage); break;
      case MessageType.PlayerColor:       matcher.PlayerColorMessage(msg as PlayerColorMessage); break;
      case MessageType.Name:              matcher.NameMessage(msg as NameMessage); break;
      case MessageType.NameTaken:         matcher.NameTakenMessage(msg as NameTakenMessage); break;
      case MessageType.NameOk:            matcher.NameOkMessage(msg as NameOkMessage); break;
      case MessageType.MaxPlayersReached: matcher.MaxPlayersReachedMessage(msg as MaxPlayersReachedMessage); break;
    }
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

  export interface NameMessage extends Message {
    readonly name: string;
  }

  export function createNameMessage(name: string): NameMessage {
    return {
      type: MessageType.Name,
      name: name,
    };
  }

  export interface NameTakenMessage extends Message {
    readonly name: string;
  }

  export interface NameOkMessage extends Message {
  }

  export interface MaxPlayersReachedMessage extends Message {
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
