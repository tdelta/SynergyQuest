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

import { DiagnosticsMessageFormat } from './DiagnosticsMessage';
import { boundClass } from 'autobind-decorator';

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
@boundClass
export class DiagnosticsClient {
  /**
   * Retrieves diagnostic information about a running game.
   *
   * @param address The network address where the game is running
   * @param port    Port where the game is listening for connections (optional, default: 8000)
   * @param timeout time after which to abort if no data has been received from the game yet
   * @returns promise with the diagnostic data
   */
  public static getDiagnostics(
    address: string,
    port: number = 8000,
    timeout: number = 1000
  ): Promise<DiagnosticsMessageFormat.DiagnosticsMessage> {
    var promise = new Promise<DiagnosticsMessageFormat.DiagnosticsMessage>(
      (resolve, reject) => {
        // Connect to game...
        var socket = new WebSocket(`wss://${address}:${port}/diagnostics/`);

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
