// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

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
