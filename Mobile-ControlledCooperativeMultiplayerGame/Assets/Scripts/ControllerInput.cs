using System;
using UnityEngine;

using WebSocketSharp;
using WebSocketSharp.Server;

public class Connection : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(e.Data);
    }
}

public class ControllerInput : MonoBehaviour
{
    private WebSocketServer wss;
    
    void Log(String str) {
        Debug.Log("ControllerInput: " + str);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        Log("Starting HTTP server on port 4242...");

        wss = new WebSocketServer("ws://127.0.0.1:4242");
        wss.AddWebSocketService<Connection>("/sockets");
        wss.Start();
    }

    void OnDisable()
    {
        wss.Stop();
    }
}
