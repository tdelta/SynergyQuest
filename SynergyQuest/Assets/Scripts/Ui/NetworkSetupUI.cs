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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

/**
 * Tries to retrieve all local network addresses and passes them to the LobbyMenu scene.
 */
public class NetworkSetupUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CheckNetworkAvailability();
    }

    public void OnRetryButton()
    {
        CheckNetworkAvailability();
    }

    /**
     * Check if there are viable ip addresses. If so, the LobbyMenu scene is loaded
     */
    private void CheckNetworkAvailability()
    {
        var ips = GetIPs();
        var addrLogStr = String.Join(", ", ips);
        Debug.Log($"Got the following local addresses: {addrLogStr}");

        if (ips.Any())
        {
            SceneController.Instance.LoadLobbyMenu(ips);
        }
    }
    
    /**
     * Retrieves a list of all available IPv4 addresses which are not associated with a loopback interface. 
     */
    private List<string> GetIPs()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(iface =>
                iface.NetworkInterfaceType != NetworkInterfaceType.Loopback
                && iface.OperationalStatus == OperationalStatus.Up
            )
            .SelectMany(iface => iface.GetIPProperties().UnicastAddresses)
            // Make sure we get IPv4s:
            .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
            .Select(addr => addr.Address.ToString())
            .ToList();
    }
}
