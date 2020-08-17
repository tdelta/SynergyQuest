using System;
using System.Net;
using System.Text;
using Boo.Lang.Runtime;
using JetBrains.Annotations;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class BaseServer
{
    [CanBeNull] private readonly WebSocketServer _wss;
    [CanBeNull] private readonly WebSocketSharp.Server.HttpServer _httpServerBackend;

    // Bind to all local addresses (0.0.0.0)
    private const string Address = "0.0.0.0";

    private string _url;

    public BaseServer()
    {
        var secure = !ServerSettings.Instance.UseSslOnlyInProductionMode || !DebugSettings.Instance.DebugMode;
        var runHttpServer = !ServerSettings.Instance.RunHttpServerOnlyInProductionMode || !DebugSettings.Instance.DebugMode;

        var protocol = runHttpServer ?
            (secure ? "https" : "http") :
            (secure ? "wss" : "ws");

        var port = runHttpServer ? ServerSettings.Instance.HttpPort : ServerSettings.Instance.WebsocketPort;

        _url = $"{protocol}://{Address}:{port}";
        
        if (!runHttpServer)
        {
            Log("Using web socket server only (because we are running in debug/development mode). Use a separate web server to server the controller software and a proxy to redirect websocket traffic here.");
            _wss = new WebSocketServer(_url);

            if (secure)
            {
                Log("Using SSL.");
                _wss.SslConfiguration.ServerCertificate = ServerSettings.Instance.RetrieveCertificate();
            }
        }

        else
        {
            var documentRoot = ServerSettings.Instance.DocumentRoot;
            
            Log($"Serving websockets and web content on single port. Using the following document root: {documentRoot}");
            _httpServerBackend = new WebSocketSharp.Server.HttpServer(_url);

            if (DebugSettings.Instance.DebugMode)
            {
                _httpServerBackend.Log.Level = LogLevel.Trace;
            }
            
            _httpServerBackend.DocumentRootPath = documentRoot;

            if (secure)
            {
                Log("Using SSL.");
                _httpServerBackend.SslConfiguration.ServerCertificate = ServerSettings.Instance.RetrieveCertificate();
            }
            
            _httpServerBackend.OnGet += (sender, e) => {
                // based on https://github.com/sta/websocket-sharp/blob/master/Example3/Program.cs
                var req = e.Request;
                var res = e.Response;
    
                var path = req.RawUrl;
                if (path == "/")
                  path += "index.html";
    
                byte[] contents;
                if (!e.TryReadFile (path, out contents)) {
                  res.StatusCode = (int) HttpStatusCode.NotFound;
                  return;
                }
    
                if (path.EndsWith (".html")) {
                  res.ContentType = "text/html";
                  res.ContentEncoding = Encoding.UTF8;
                }
                else if (path.EndsWith (".js")) {
                  res.ContentType = "application/javascript";
                  res.ContentEncoding = Encoding.UTF8;
                }
    
                res.ContentLength64 = contents.LongLength;
                res.Close (contents, true);
            };
        }
    }

    public void Start()
    {
        Log($"Starting server on {_url}...");
        _wss?.Start();
        _httpServerBackend?.Start();
    }

    public void Stop()
    {
        Log($"Stopping server...");
        _wss?.Stop();
        _httpServerBackend?.Stop();
    }
    
    public void AddWebSocketService<TBehaviorWithNew>(
      string path,
      Action<TBehaviorWithNew> initializer
  )
      where TBehaviorWithNew : WebSocketBehavior, new()
    {
      _wss?.AddWebSocketService<TBehaviorWithNew>(path, initializer);
      _httpServerBackend?.AddWebSocketService<TBehaviorWithNew>(path, initializer);
    }
    
    public void AddWebSocketService<TBehaviorWithNew>(string path) where TBehaviorWithNew : WebSocketBehavior, new()
    {
      _wss?.AddWebSocketService<TBehaviorWithNew>(path);
      _httpServerBackend?.AddWebSocketService<TBehaviorWithNew>(path);
    }

    public WebSocketServiceManager WebSocketServices
    {
        get
        {
            if (_httpServerBackend != null)
            {
                return _httpServerBackend.WebSocketServices;
            }

            else if (_wss != null)
            {
                return _wss.WebSocketServices;
            }

            throw new RuntimeException("No websocket service provider available. This should never happen and is a programming error.");
        }
    }
    
    private static void Log(String str) {
        Debug.Log($"[Server] {str}");
    }
}
