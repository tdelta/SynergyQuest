import WebSocket from 'isomorphic-ws';

import { MessageFormat } from './Message';

/**
 * Stores the ids of the different controller buttons
 */
export enum Button {
  Attack = 0,
  Pull = 1,
  Press = 3,
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
 * Different reasons why the game rejected a connection
 */
export enum ConnectFailureReason {
  NameAlreadyTaken, // A controller is already connected with the same player name
  MaxPlayersReached, // The maximum amount of players is already connected to the game
}

/**
 * Allows to connect to a Coop-Dungeon game and send controller inputs
 * or receive messages from the game.
 *
 * Internally, this class sends and receives JSON encoded messages over
 * websockets.  For the format of the websockets, see the `Message` class
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
   * Set of all currently enabled menu actions.
   */
  private enabledMenuActions: Set<MenuAction> = new Set<MenuAction>();

  /**
   * Set of all currently enabled menu actions. (e.g. pressing a button of reading a sign)
   */
  private enabledGameActions: Set<Button> = new Set<Button>();

  /**
   * Current state of the game. E.g. Lobby or Started
   */
  private gameState: GameState = GameState.Lobby;

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
   * enables a game action to be displayed as a button in the controller ui
   */
  public onSetGameAction: (action: Button, enabled: boolean) => any;
  
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
  public constructor() {
    this.ensureReady = this.ensureReady.bind(this);
    this.sendMessage = this.sendMessage.bind(this);
    this.handleSocketOpen = this.handleSocketOpen.bind(this);
    this.handleSocketClose = this.handleSocketClose.bind(this);
    this.handleSocketError = this.handleSocketError.bind(this);
    this.handleMessage = this.handleMessage.bind(this);
    this.setJoystickPosition = this.setJoystickPosition.bind(this);
    this.setButton = this.setButton.bind(this);
    this.getColor = this.getColor.bind(this);
  }

  /**
   * Connects to the game.
   * Be aware that the client is not immediately connected.
   *
   * Register callbacks `onConnect`, `onDisconnect`, `onError` to keep track of
   * when the client is ready for use.
   *
   * @param name    Name of the player using this controller
   * @param address The network address where the game is running
   * @param port    Port where the game is listening for controller connections (optional, default: 4242)
   **/
  public connect(name: string, address: string, port: number = 4242) {
    // if the there is already a socket which is not closed or closing...
    if (this.socket?.readyState !== 2 && this.socket?.readyState !== 3) {
      // then close it first
      this.socket?.close();
    }

    // Create a new websocket and connect to the game
    this.socket = new WebSocket(`ws://${address}:${port}/sockets/`);

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
    const msg = MessageFormat.createJoystickMessage(vertical, horizontal);

    this.sendMessage(msg);
  }

  /**
   * Sends, whether a button has been pressed or released.
   *
   * @param button identifier of the button, see `Button` enum
   * @param onOff  whether the button is pressed (`true`) or not (`false`)
   */
  public setButton(button: Button, onOff: boolean) {
    this.ensureReady();
    const msg = MessageFormat.createButtonMessage(button, onOff);

    this.sendMessage(msg);
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
      var msg = MessageFormat.createMenuActionTriggeredMessage(action);

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
   * Returns all game actions (e.g. pressing a button or reading a sign)
   * which are currently enabled by the game.
   * Use the `onSetGameAction` event to keep track of when actions are enabled
   * or disabled.
   */
  public getEnabledGameActions(): Set<Button> {
    return this.enabledGameActions;
  }

  /**
   * Returns the current state of the game. E.g. "Lobby" or "Started".
   * Use the `onGameStateChanged` event to keep track of when the state changes.
   */
  public getGameState(): GameState {
    return this.gameState;
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

      SetGameActionMessage: msg => {
        if (msg.enabled) {
          this.enabledGameActions.add(msg.button);
        } else if (this.enabledGameActions.has(msg.button)) {
          this.enabledGameActions.delete(msg.button);
        }

        this.onSetGameAction?.(msg.button, msg.enabled);
      },
      
      VibrationSequenceMessage: msg => {
        this.onVibrationRequest?.(msg.vibrationPattern);
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
