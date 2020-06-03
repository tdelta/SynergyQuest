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
