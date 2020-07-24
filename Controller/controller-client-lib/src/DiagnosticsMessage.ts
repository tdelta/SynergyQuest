/**
 * This namespace describes the format of messages sent from the diagnostic
 * service of the game and provides utilities to deserialize them.
 *
 * Similar code can be found in the Unity game in the file
 * `DiagnosticsMessage.cs`.
 */
export namespace DiagnosticsMessageFormat {
  export interface DiagnosticsMessage {
    /**
     * Names of those players who lost their connection to the game
     */
    readonly playersWithLostConnection: string[];
  }

  export function messageFromJSON(str: string): DiagnosticsMessage {
    const msgObj = JSON.parse(str);

    return msgObj;
  }
}
