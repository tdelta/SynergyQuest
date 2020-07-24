import WebSocket from 'isomorphic-ws';

import { DiagnosticsMessageFormat } from './DiagnosticsMessage';

/**
 * Allows to connect to the diagnostics service of a Coop-Dungeon game and
 * retrieve information.
 *
 * For example this information can be used to determine, whether there is a
 * running game to which the client lost connection and can reconnect.
 *
 * Internally, this class receives a JSON encoded message over a
 * websocket from the game upon connection.
 * For the format of the message, see the `DiagnosticsMessage` class.
 *
 * For an usage example, see the `controller-app` or the
 * `controller-lib-test-app`.
 */
export class DiagnosticsClient {
  /**
   * Retrieves diagnostic information about a running game.
   *
   * @param address The network address where the game is running
   * @param port    Port where the game is listening for connections (optional, default: 4242)
   * @param timeout time after which to abort if no data has been received from the game yet
   * @returns promise with the diagnostic data
   */
  public static getDiagnostics(
    address: string,
    port: number = 4242,
    timeout: number = 1000
  ): Promise<DiagnosticsMessageFormat.DiagnosticsMessage> {
    var promise = new Promise<DiagnosticsMessageFormat.DiagnosticsMessage>(
      (resolve, reject) => {
        // Connect to game...
        var socket = new WebSocket(`ws://${address}:${port}/diagnostics/`);

        // This variable will record, whether we already retrieved information
        // from the server and resolved the promise,
        // or whether the connection failed for some reason and the promise
        // already got rejected.
        var hasResolved = false;

        // Generates a function which will let this promise fail
        var fail = (reason: string) => (_: any) => {
          // If the promise has not already been resolved/rejected..
          if (!hasResolved) {
            // mark it as rejected
            hasResolved = true;
            // reject the promise
            reject(reason);
            socket.close();
          }
        };

        socket.onclose = fail(
          'Connection closed before a diagnostics message arrived'
        );

        socket.onerror = fail('The connection experienced an error');

        socket.onmessage = (msgEvent: MessageEvent) => {
          // If the promise has not already been resolved / rejected...
          if (!hasResolved) {
            // Parse the incoming data
            const msg = DiagnosticsMessageFormat.messageFromJSON(msgEvent.data);

            // Mark the promise as resolved
            hasResolved = true;
            // Pass on the diagnostic data
            resolve(msg);
            socket.close();
          }
        };

        setTimeout(
          fail('Timed out before a diagnostic message arrived'),
          timeout
        );
      }
    );

    return promise;
  }
}
