using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

using System.Reflection;
using System.Runtime.Versioning;

public class ControllerInput : MonoBehaviour
{
    // references: https://stackoverflow.com/a/35896157
    HttpListener httpListener = new HttpListener();

    void Log(String str) {
        Debug.Log("ControllerInput: " + str);
    }

    // Start is called before the first frame update
    void Start()
    {
        Log("Starting HTTP server on port 4242...");

        httpListener.Prefixes.Add("http://127.0.0.1:8080/sockets/");
        httpListener.Start();

        StartCoroutine("ListenForConnection");
    }

    async void ListenForConnection() {
        Log("Listening for a websocket connection...");

        // Wait for an incoming connection
        HttpListenerContext context = await httpListener.GetContextAsync();

        // Check, if it is a websocket connection
        if (context.Request.IsWebSocketRequest)
        {
            Log("Got a websocket connection from " + context.Request.LocalEndPoint);

            HttpListenerWebSocketContext webSocketContext =
                await context.AcceptWebSocketAsync(
                    null // dont specify a websocket sub-protocol
                );

            WebSocket webSocket = webSocketContext.WebSocket;
            StartCoroutine("HandleConnection", webSocket);
        }

        else {
            Log("Incoming connection was not a websocket connection!");
            context.Response.StatusCode = 400;
            context.Response.Close();
        }

        ListenForConnection();
    }

    async void HandleConnection(WebSocket webSocket) {
        while (webSocket.State == WebSocketState.Open) {
            var receiveBuffer = new ArraySegment<byte>();
            var messageBuffer = new MemoryStream();
            WebSocketReceiveResult result = null;

            do {
                result = await webSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);

                messageBuffer.Write(receiveBuffer.Array, receiveBuffer.Offset, result.Count);
            } while(!result.EndOfMessage);

            messageBuffer.Seek(0, SeekOrigin.Begin);

            if (result.MessageType == WebSocketMessageType.Text) {
                var strMessage = System.Text.Encoding.UTF8.GetString(
                    messageBuffer.GetBuffer(), 0, (int) messageBuffer.Length
                );

                Log("Got websocket message: " + strMessage);
            }

            else {
                Log("Received a websocket message, but it was not text!");
            }
        }

        Log("Websocket is no longer open. Abandoning connection.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
