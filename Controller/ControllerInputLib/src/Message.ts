import { Button } from './ControllerInputSender';

enum MessageType {
  Button = 0,
  Joystick = 1
}

/**
 * This class hierarchy determines the format of the JSON messages sent
 * between the game controller and the game.
 *
 * The base class only carries an identifier which determines the type of the
 * message. See also the `MessageType` enum.
 */
class Message {
  readonly type: MessageType

  protected constructor(type: MessageType) {
    this.type = type;
  }
}

export class ButtonMessage extends Message {
  readonly button: Button;
  readonly onOff: boolean;

  constructor(button: Button, onOff: boolean) {
    super(MessageType.Button);
    this.button = button;
    this.onOff = onOff;
  }
}

export class JoystickMessage extends Message {
  readonly vertical: number;
  readonly horizontal: number;

  constructor(vertical: number, horizontal: number) {
    super(MessageType.Joystick);
    this.vertical = vertical;
    this.horizontal = horizontal;
  }
}
