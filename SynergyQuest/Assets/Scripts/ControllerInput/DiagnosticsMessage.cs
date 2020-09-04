using System;
using System.Collections.Generic;
using UnityEngine;


/**
 * Format of the messages sent by `DiagnosticsConnection`.
 */
[Serializable]
public class DiagnosticsMessage
{
    /**
     * Names of those players who lost their connection to the game
     */
    public List<string> playersWithLostConnection;

    /**
     * Serialize this message into JSON.
     */
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

