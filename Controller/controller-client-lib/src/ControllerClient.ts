/**
 * This file is part of the "Synergy Quest" game
 * (github.com/tdelta/SynergyQuest).
 *
 * Copyright (c) 2020
 *   Marc Arnold     (m_o_arnold@gmx.de)
 *   Martin Kerscher (martin_x@live.de)
 *   Jonas Belouadi  (jonas.belouadi@posteo.net)
 *   Anton W Haubner (anton.haubner@outlook.de)
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published by the Free
 * Software Foundation; either version 3 of the License, or (at your option) any
 * later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
 * more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * this program; if not, see <https://www.gnu.org/licenses>.
 *
 * Additional permission under GNU GPL version 3 section 7 apply,
 * see `LICENSE.md` at the root of this source code repository.
 */

import WebSocket from 'isomorphic-ws';

import { MessageFormat } from './Message';
import { boundClass } from 'autobind-decorator';

/**
 * Stores the ids of the different controller buttons
 */
export enum Button {
  Attack = 0,
  Pull = 1,
  Carry = 2,
  Press = 3,
  Throw = 4,
  Read = 5,
  Open = 6,
  UseBomb = 7,
  Exit = 8,
  JumpBack = 9, // used by Springs, see also Spring behaviour
}

/**
 * Identifiers of the different "menu actions" which can be enabled/disabled for controllers
 */
export enum MenuAction {
  StartGame = 0,
  QuitGame = 1,
  PauseGame = 2,
  ResumeGame = 3,
  Next = 4, // Info screens can have multiple pages, which can be browsed with this menu action
  Back = 5,
  Yes = 6,
  No = 7,
  ShowMap = 8,
}

export enum PlayerColor {
  Red = 0,
  Blue = 1,
  Green = 2,
  Yellow = 3,
  Any = 4, // May interact with any object.
}

/**
 * Models current state of the game.
 * For example, the game may still be displaying the lobby or be already running.
 */
export enum GameState {
  Lobby = 0,
  Started = 1,
  Menu = 2,
}

/**
 * Hint to controller, what inputs are currently expected from this specific controller by the game.
 *
 * For example, if the mode is set to `IMUOrientation`, the controller should adapt its display to give the user
 * visual feedback about the 3d orientation of the device. On the other hand, it does not need to display the joystick
 * and most buttons, since joystick input will be ignored by the game.
 *
 * This is different from `GameState` which is global to all controllers and also indicates the overall
 * state of the game.
 */
export enum InputMode {
  // input from all buttons, menu actions, joystick etc.
  Normal = 0,
  // orientation input from IMU sensors is expected. Controller does not need to display joystick or buttons (except the `Exit` button). Menus should still be displayed
  IMUOrientation = 1,
  /**
   * The player character belonging to this controller has died, and is now undergoing the "Revival Minigame".
   * See also RevivalMinigame class in the main game code.
   *
   * The controller should display information on how to solve the minigame.
   * The controller does not need to display joystick or buttons. Menus should still be displayed
   */
  RevivalMinigame = 2,
}

/**
 * Different reasons why the game rejected a connection
 */
export enum ConnectFailureReason {
  NameAlreadyTaken, // A controller is already connected with the same player name
  MaxPlayersReached, // The maximum amount of players is already connected to the game
}

export interface PlayerInfo {
  HealthPoints: number;
  Gold: number;
}

/**
 * Allows to connect to a Coop-Dungeon game and send controller inputs
 * or receive messages from the game.
 *
 * Internally, this class sends and receives JSON encoded messages over
 * websockets.  For the format of the messages, see the `Message` class
 * hierarchy.
 *
 * Usage example:
 * ```typescript
 *   import { ControllerClient, Button } from controller_client_lib;
 *
 *   let client = new ControllerClient();
 *
 *   client.onReady = () => {
 *     ...
 *     client.setButton(Button.Attack, true);
 *     ...
 *   };
 *
 *   client.onConnectFailure = () => { ... };
 *   ...
 *
 *   client.connect('Max Muster', '127.0.0.1');
 * ```
 *
 * For further examples, see the few tests in the folder `__tests__` or the
 * example application `controller-lib-test-app`.
 */
@boundClass
export class ControllerClient {
  private socket: WebSocket.WebSocket;

  /**
   * Indicates, whether a connection has been established fully.
   * That is, the game is now ready to receive inputs.
   *
   * This is set after the game accepted the player name etc.
   */
  private ready = false;

  /**
   * Player color assigned by the game. May be undefined until the server
   * sends a colour.
   */
  private color?: PlayerColor;

  /**
   * We cache the last input in these variables so that we can check whether
   * an input value actually changed before sending it.
   *
   * This way we keep the number of sent messages to a minimum.
   */
  private joystickVerticalCache: number = 0;
  private joystickHorizontalCache: number = 0;
  private imuOrientationVerticalCache: number = 0;
  private imuOrientationHorizontalCache: number = 0;
  private pressedButtonsCache: Set<Button> = new Set<Button>();

  /**
   * Set of all currently enabled menu actions.
   */
  private enabledMenuActions: Set<MenuAction> = new Set<MenuAction>();

  /**
   * Set of all currently enabled buttons.
   * (e.g. the read button will only be enabled when the player is in front of
   *  a sign)
   */
  private enabledButtons: Set<Button> = new Set<Button>();

  /**
   * Set of all buttons currently cooling down.
   * (e.g. bombs can only be used every n seconds. Hence the game informs the
   *  controller in the meantime, that the the bomb button is cooling down.)
   *
   * => Buttons which are cooling down are still enabled, but have no effect
   *    and should be displayed differently.
   */
  private cooldownButtons: Set<Button> = new Set<Button>();

  /**
   * Information about the player (health, gold) that should be
   * displayed on the controller
   */
  private playerInfo?: PlayerInfo = undefined;

  /**
   * Current state of the game. E.g. Lobby or Started
   */
  private gameState: GameState = GameState.Lobby;

  /**
   * Current input mode the game expects from the controller, see description of
   * `InputMode`.
   */
  private inputMode: InputMode = InputMode.Normal;

  /**
   * Callback which can be set by users and which is called if the server
   * refused to establish a connection for the given reason.
   */
  public onConnectFailure: (reason: ConnectFailureReason) => any;

  /**
   * Callback which can be set by users and which is called when a connection to
   * a game has been fully established so that it is ready to receive inputs.
   */
  public onReady: () => any;

  /**
   * Callback which can be set by users and which is called when the connection
   * to a game has been closed
   */
  public onDisconnect: () => any;

  /**
   * Callback which can be set by users and which is called when the connection
   * to a game experienced some sort of error.
   */
  public onError: () => any;

  /**
   * Callback which can be set by users and which is called whenever the server
   * assigns this client a color.
   */
  public onSetPlayerColor: (color: PlayerColor) => any;

  /**
   * Callback which can be set by users and which is called whenever the game
   * enables or disables some menu action for the controller
   */
  public onSetEnabledMenuActions: (enabledActions: Set<MenuAction>) => any;

  /**
   * Callback which can be set by users and which is called whenever the game
   * changes its state, e.g. from "Lobby" to "Started"
   */
  public onGameStateChanged: (state: GameState) => any;

  /**
   * Callback which can be set by users and which is called whenever the game
   * changes the controllers' input mode, e.g. from "Normal" to "IMUControlled"
   */
  public onInputModeChanged: (inputMode: InputMode) => any;

  /**
   * Callback which can be set by users and which is called whenever the game
   * enables/disables some button for the controller.
   */
  public onSetEnabledButtons: (enabledButtons: Set<Button>) => any;

  /**
   * Callback which can be set by users and which is called whenever the game
   * tells the controller that some button is cooling down / not cooling down
   * anymore
   */
  public onSetCooldownButtons: (cooldownButtons: Set<Button>) => any;

  /**
   * Callback which can be set by users and which is called whenever the game
   * updates information that the controller should display (gold, health)
   */
  public onSetPlayerInfo: (playerInfo: PlayerInfo) => any;

  /**
   * Callback which can be set by users and which is called whenever the game
   * wants the controller to vibrate.
   *
   * @param vibrationPattern alternating number of milliseconds to vibrate and pause
   */
  public onVibrationRequest: (vibrationPattern: number[]) => any;

  /**
   * Creates a ControllerClient instance.
   *
   * After creating an instance, set the callbacks you want to use and then call
   * the `connect` method.
   */
  // eslint-disable-next-line no-useless-constructor
  public constructor() {}

  /**
   * Connects to the game.
   * Be aware that the client is not immediately connected.
   *
   * Register callbacks `onConnect`, `onDisconnect`, `onError` to keep track of
   * when the client is ready for use.
   *
   * @param name    Name of the player using this controller
   * @param address The network address where the game is running
   * @param port    Port where the game is listening for controller connections (optional, default: 8000)
   **/
  public connect(name: string, address: string, port: number = 8000) {
    // if the there is already a socket which is not closed or closing...
    if (this.socket?.readyState !== 2 && this.socket?.readyState !== 3) {
      // then close it first
      this.socket?.close();
    }

    // Create a new websocket and connect to the game
    this.socket = new WebSocket(`wss://${address}:${port}/controllers/`);

    // Set all event handlers of the socket to local methods
    this.socket.onopen = (_: Event) => this.handleSocketOpen(name);
    this.socket.onclose = (_: CloseEvent) => this.handleSocketClose();
    this.socket.onerror = (_: Event) => this.handleSocketError();
    this.socket.onmessage = this.handleMessage;
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
      throw Error(
        'Only joystick positions in the interval [-1; 1] are allowed.'
      );
    }

    this.ensureReady();

    // Only send new position, if it actually changed
    if (
      vertical !== this.joystickVerticalCache ||
      horizontal !== this.joystickHorizontalCache
    ) {
      const msg = MessageFormat.createJoystickMessage(vertical, horizontal);

      this.sendMessage(msg);

      this.joystickVerticalCache = vertical;
      this.joystickHorizontalCache = horizontal;
    }
  }

  /**
   * Sends a 2d position based on the 3d orientation of the controller device
   * This should be implemented by interpreting the "roll" and "pitch orientation:
   * https://en.wikipedia.org/wiki/Euler_angles
   * https://en.wikipedia.org/wiki/Aircraft_principal_axes
   *
   * @param vertical    2d vertical position interpreted from a 3d rotation
   *                    position of the controller (roll).
   *                    Must be a floating point number in [-1; 1]
   * @param horizontal  2d horizontal position interpreted from a 3d rotation
   *                    position of the controller (pitch).
   *                    Must be a floating point number in [-1; 1]
   */
  public setImuOrientation(vertical: number, horizontal: number) {
    if (vertical < -1 || vertical > 1 || horizontal < -1 || horizontal > 1) {
      throw Error(
        'Only orientation positions in the interval [-1; 1] are allowed.'
      );
    }

    this.ensureReady();

    // Only send new position, if it actually changed
    if (
      vertical !== this.imuOrientationVerticalCache ||
      horizontal !== this.imuOrientationHorizontalCache
    ) {
      const msg = MessageFormat.createIMUOrientationMessage(
        vertical,
        horizontal
      );

      this.sendMessage(msg);

      this.imuOrientationVerticalCache = vertical;
      this.imuOrientationHorizontalCache = horizontal;
    }
  }

  /**
   * Sends, whether a button has been pressed or released.
   *
   * @param button identifier of the button, see `Button` enum
   * @param onOff  whether the button is pressed (`true`) or not (`false`)
   */
  public setButton(button: Button, onOff: boolean) {
    this.ensureReady();

    // Only send button value, if it actually changed
    if (onOff !== this.pressedButtonsCache.has(button)) {
      const msg = MessageFormat.createButtonMessage(button, onOff);

      this.sendMessage(msg);

      if (onOff) {
        this.pressedButtonsCache.add(button);
      } else {
        this.pressedButtonsCache.delete(button);
      }
    }
  }

  /**
   * Returns the color assigned to this controller by the game.
   * May be undefined, if the game has not sent a color yet.
   *
   * To be notified when a color is assigned, set the callback
   * `onSetPlayerColor`.
   */
  public getColor(): PlayerColor | undefined {
    return this.color;
  }

  /**
   * Informs the game that a menu action has been triggered.
   *
   * @throws Error if the menu action has not been enabled by the game or if the connection to the game is currently not ready
   */
  public triggerMenuAction(action: MenuAction) {
    if (!this.enabledMenuActions.has(action)) {
      throw Error(
        'Can not trigger menu option which the game has not enabled.'
      );
    } else {
      this.ensureReady();
      const msg = MessageFormat.createMenuActionTriggeredMessage(action);

      this.sendMessage(msg);
    }
  }

  /**
   * Returns all menu actions which are currently enabled by the game.
   * Use the `onSetMenuAction` event to keep track of when actions are enabled
   * or disabled.
   */
  public getEnabledMenuActions(): Set<MenuAction> {
    return this.enabledMenuActions;
  }

  /**
   * Returns all buttons which are currently enabled by the game.
   * Use the `onSetEnabledButtons` event to keep track of when buttons are
   * enabled or disabled.
   */
  public getEnabledButtons(): Set<Button> {
    return this.enabledButtons;
  }

  /**
   * Set of all buttons currently cooling down.
   * (e.g. bombs can only be used every n seconds. Hence the game informs the
   *  controller in the meantime, that the the bomb button is cooling down.)
   *
   * => Buttons which are cooling down are still enabled, but have no effect
   *    and should be displayed differently.
   *
   * Use the `onSetCooldownButtons` event to keep track of when buttons are
   * enabled or disabled.
   */
  public getCooldownButtons(): Set<Button> {
    return this.cooldownButtons;
  }

  /**
   * Returns the current state of the game. E.g. "Lobby" or "Started".
   * Use the `onGameStateChanged` event to keep track of when the state changes.
   */
  public getGameState(): GameState {
    return this.gameState;
  }

  /**
   * Returns the input mode currently expected by the game from the controller.
   * E.g. "Normal" or "IMUControlled". See also the description of `InputMode`.
   *
   * Use the `onInputModeChanged` event to keep track of when the mode changes.
   */
  public getInputMode(): InputMode {
    return this.inputMode;
  }

  /**
   * Returns information about the player (health, gold) that the controller should display
   */
  public getPlayerInfo(): PlayerInfo | undefined {
    return this.playerInfo;
  }

  /**
   * Returns whether the client is fully connected to a game and can send
   * inputs.
   *
   * DO NOT check this function periodically to determine if a connection is
   * established yet. Instead set the `onReady` and `onDisconnect` callbacks.
   */
  public isReady(): boolean {
    return this.ready;
  }

  private isConnected(): boolean {
    return this.socket.readyState === 1;
  }

  /**
   * Called as soon as a raw websocket connection has been established.
   */
  private handleSocketOpen(name: string) {
    // A client must first send a player name before anything else.
    // We do this, as soon as a raw websocket connection has been established.
    this.sendMessage(MessageFormat.createNameMessage(name));
  }

  /**
   * Called if the websocket connection to the game has been closed.
   */
  private handleSocketClose() {
    this.ready = false;
    this.onDisconnect?.();
  }

  /**
   * Called if some sort of error happened with the websocket connection.
   */
  private handleSocketError() {
    console.error('ControllerClient: Connection to game experienced an error.');
    this.onError?.();
  }

  /**
   * Handles messages sent by the server
   *
   * @param msgEvent message event received over the websocket, see also `WebSocket.onmessage`.
   */
  private handleMessage(msgEvent: MessageEvent) {
    // deserialize the message from JSON
    const msg = MessageFormat.messageFromJSON(msgEvent.data);

    // For each kind of message do something different:
    // (This simulates ADT match functions as known from Haskell or Scala)
    MessageFormat.matchMessage(msg, {
      ...MessageFormat.defaultMatcher, // <- for all kinds of messages, do nothing but for the ones listed below

      NameTakenMessage: _ => {
        // The game says someone is already using this player name.
        // Establishing a connection has failed then and must be retried.
        this.socket.close();
        this.onConnectFailure?.(ConnectFailureReason.NameAlreadyTaken);
      },

      MaxPlayersReachedMessage: _ => {
        // The game says the maximum number of players is already connected.
        // Establishing a connection has failed then and must be retried.
        this.socket.close();
        this.onConnectFailure?.(ConnectFailureReason.MaxPlayersReached);
      },

      NameOkMessage: _ => {
        // The game has accepted our player name and the connection is now fully
        // established.
        this.ready = true;
        this.onReady?.();
      },

      PlayerColorMessage: msg => {
        // If the server sent a player color, set the color and call the callback
        // `onSetPlayerColor` which can be set by users of the library.
        this.color = msg.color;
        this.onSetPlayerColor?.(msg.color);
      },

      SetMenuActionsMessage: msg => {
        this.enabledMenuActions = new Set(msg.menuActions);
        this.onSetEnabledMenuActions?.(this.enabledMenuActions);
      },

      GameStateChangedMessage: msg => {
        this.gameState = msg.gameState;

        this.onGameStateChanged?.(msg.gameState);
      },

      InputModeChangedMessage: msg => {
        this.inputMode = msg.inputMode;

        this.onInputModeChanged?.(msg.inputMode);
      },

      SetEnabledButtonsMessage: msg => {
        this.enabledButtons = new Set(msg.enabledButtons);
        this.onSetEnabledButtons?.(this.enabledButtons);
      },

      SetCooldownButtonsMessage: msg => {
        this.cooldownButtons = new Set(msg.cooldownButtons);
        this.onSetCooldownButtons?.(this.cooldownButtons);
      },

      VibrationSequenceMessage: msg => {
        this.onVibrationRequest?.(msg.vibrationPattern);
      },

      PlayerInfoMessage: msg => {
        this.playerInfo = msg.playerInfo;
        this.onSetPlayerInfo?.(this.playerInfo);
      },
    });
  }

  /**
   * Converts a message to a JSON string and sends it to the game over the
   * websocket.
   *
   * It throws an exception if we are not connected.
   */
  private sendMessage(msg: MessageFormat.Message) {
    if (this.isConnected()) {
      this.socket.send(JSON.stringify(msg));
    } else {
      throw new Error('Can not send message if client is not connected.');
    }
  }

  /**
   * Should be called before sending any kind of message which may only be sent
   * if a connection has been fully established.
   *
   * It ensures the connection is ready to send inputs by throwing an exception
   * otherwise.
   */
  private ensureReady() {
    if (!this.ready) {
      throw Error(
        'Can not interact with game since the connection is not ready yet.'
      );
    }
  }
}
