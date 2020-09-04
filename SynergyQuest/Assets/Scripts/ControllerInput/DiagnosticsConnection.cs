using System.Linq;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

/**
 * Sends diagnostic information about the game to clients upon connection and then immediately disconnects.
 * 
 * For example, remote controllers can use this information to determine, whether there is a running game to which they
 * lost connection and can reconnect.
 *
 * The information is sent as a JSON string in the format dictated by the `DiagnosticsMessage` class.
 */
public class DiagnosticsConnection : WebSocketBehavior
{
    protected override void OnOpen()
    {
        var inputs = ControllerServer.Instance.GetInputs();
        
        var msg = new DiagnosticsMessage()
        {
            // Report, which players currently lost their connection
            playersWithLostConnection =
                inputs.Where(input => !input.IsConnected()).Select(input => input.PlayerName).ToList()
        };
        
        // Send the diagnostic information
        Send(msg.ToJson());
        // Close the connection
        Close();
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError($"Connection: Error in diagnostic connection: {e.Message}");
    }
}
