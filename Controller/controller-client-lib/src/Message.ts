import { Button, MenuAction, PlayerColor } from './ControllerClient';

/**
 * This namespace describes the format of messages sent between controller and
 * game and provides utilities to create, (de-)serialize and handle messages.
 *
 * It is only used internally by the client library and is not intended to be
 * used directly.
 *
 * Similar code can be found in the Unity game in the file `Message.cs`
 */
export namespace MessageFormat {
  /**
   * The different kinds of messages
   */
  export enum MessageType {
    None = 0,               // Placeholder type for base class
    Name = 1,               // Name of the player, sent by controller upon establishing a connection
    NameTaken = 2,          // Game rejects player name since it is already used by another controller, sent by game
    NameOk = 3,             // Game accepts player name, sent by game
    MaxPlayersReached = 4,  // Game rejects connection since the maximum number of controllers is already connected to it, sent by game
    Button = 5,             // Button input, sent by controller
    Joystick = 6,           // Joystick input, sent by controller
    PlayerColor = 7,        // Color assigned to a player, sent by the game
    SetMenuAction = 8,      // Enable / disable a menu action, sent by game
    MenuActionTriggered = 9 // A menu action has been selected on the controller
  }

  /**
   * This interface hierarchy determines the format of the JSON messages sent
   * between the game controller and the game.
   *
   * The base interface only carries an identifier which determines the type of the
   * message. See also the `MessageType` enum.
   *
   * The Message Types can be used a bit like an ADT, see the `matchMessage` function
   * down below.
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
    readonly color: PlayerColor; // hexadecimal number with leading #, e.g. "#c0ffee"
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

  export interface SetMenuActionMessage extends Message {
    readonly menuAction: MenuAction;
    readonly enabled: boolean;
  }

  export interface MenuActionTriggeredMessage extends Message {
    readonly menuAction: MenuAction;
  }

  export function createMenuActionTriggeredMessage(menuAction: MenuAction): MenuActionTriggeredMessage {
    return {
      type: MessageType.MenuActionTriggered,
      menuAction: menuAction,
    };
  }

  /**
   * Creates an object conforming to the message interfaces from a JSON encoded
   * string representation of it.
   *
   * This function is used to deserialize messages sent from the game.
   */
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

  /**
   * Using a switch-case on the message types is cumbersome since it requires
   * typecasts etc.
   *
   * Instead we simulate a "Match" functionality as it is used in functional
   * programming languages like Haskell or Scala for ADTs.
   *
   * See the `handleMessage` method of `ControllerClient` for an usage example.
   *
   * @param msg     message for which we want to execute different actions depending on the type.
   * @param matcher contains a callback action for each kind of message.
   */
  export function matchMessage(msg: Message, matcher: Matcher) {
    switch (msg.type) {
      case MessageType.Button:              matcher.ButtonMessage(msg as ButtonMessage); break;
      case MessageType.Joystick:            matcher.JoystickMessage(msg as JoystickMessage); break;
      case MessageType.PlayerColor:         matcher.PlayerColorMessage(msg as PlayerColorMessage); break;
      case MessageType.Name:                matcher.NameMessage(msg as NameMessage); break;
      case MessageType.NameTaken:           matcher.NameTakenMessage(msg as NameTakenMessage); break;
      case MessageType.NameOk:              matcher.NameOkMessage(msg as NameOkMessage); break;
      case MessageType.MaxPlayersReached:   matcher.MaxPlayersReachedMessage(msg as MaxPlayersReachedMessage); break;
      case MessageType.SetMenuAction:       matcher.SetMenuActionMessage(msg as SetMenuActionMessage); break;
      case MessageType.MenuActionTriggered: matcher.MenuActionTriggeredMessage(msg as MenuActionTriggeredMessage); break;
    }
  }

  /**
   * See `matchMessage` function for an explanation.
   */
  export interface Matcher {
    readonly ButtonMessage:              (_: ButtonMessage) => any           ;
    readonly JoystickMessage:            (_: JoystickMessage) => any         ;
    readonly PlayerColorMessage:         (_: PlayerColorMessage) => any      ;
    readonly NameMessage:                (_: NameMessage) => any             ;
    readonly NameTakenMessage:           (_: NameTakenMessage) => any        ;
    readonly NameOkMessage:              (_: NameOkMessage) => any           ;
    readonly MaxPlayersReachedMessage:   (_: MaxPlayersReachedMessage) => any;
    readonly SetMenuActionMessage:       (_: SetMenuActionMessage) => any;
    readonly MenuActionTriggeredMessage: (_: MenuActionTriggeredMessage) => any;
  }

  /**
   * Can be used to avoid setting a match action for every message type when we
   * just want to set it for some.
   *
   * See the `handleMessage` method of `ControllerClient` for an usage example.
   */
  export let defaultMatcher: Matcher = {
    ButtonMessage:              _ => {},
    JoystickMessage:            _ => {},
    PlayerColorMessage:         _ => {},
    NameMessage:                _ => {},
    NameTakenMessage:           _ => {},
    NameOkMessage:              _ => {},
    MaxPlayersReachedMessage:   _ => {},
    SetMenuActionMessage:       _ => {},
    MenuActionTriggeredMessage: _ => {}
  };
}
