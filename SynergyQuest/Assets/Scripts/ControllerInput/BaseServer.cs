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
using System.IO;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

/**
 * <summary>
 * Implements a local HTTP(S) server which serves websockets and the web based controller software.
 * Remember to call <c>Start</c> and <c>Stop</c> on an instance of this class.
 * </summary>
 * <remarks>
 * Most settings are controlled by the <c>ServerSettings</c> scriptable object, e.g.
 * 
 * * Whether SSL is used and where the certificate is located
 *   (a secure context, that is only SSL connections, are required by modern browsers to access certain sensor data
 *    on controllers, e.g. 3D orientation
 *    See also https://developer.mozilla.org/en-US/docs/Web/Security/Secure_Contexts/features_restricted_to_secure_contexts
 *   )
 * * Whether the controller software is served or only websockets
 *   (in development/debug mode you usually want to serve the controller software from a separate development server)
 * * Where the document root for the web-based controller software is located
 * * ...
 * 
 * When running in debug mode with websockets only, use the proxy server shipped with this project
 * (<c>Controllers/proxy</c>). It will divert websocket requests here and HTTP document requests to a separate web
 * development server.
 *
 * Also be aware, that regarding websockets, this class only provides access to the websocket-sharp framework for
 * implementing websockets. The websockets are actually managed by the <c>ControllerServer</c> class.
 *
 * Furthermore, when using self-signed certificates with this server, this might generate browser warnings.
 * To guide users through this process, a special web app which informs about these warnings is also served in
 * production mode by <see cref="SslWarningInfoServer"/>.
 * </remarks>
 */
public class BaseServer
{
    // In debug mode, we use this pure websocket server
    [CanBeNull] private readonly WebSocketServer _wss;

    // In production mode, we use a full HTTP server which can upgrade connections to the websocket protocol
    [CanBeNull] private readonly HttpServer _httpServerBackend;

    // Bind to all local addresses (0.0.0.0)
    private const string Address = "0.0.0.0";

    // Will store the full server URL after start depending on the <c>ServerSettings</c>
    // E.g. "https://0.0.0.0:8000"
    private string _url;

    /**
     * <summary>
     * Reports the port used by this server.
     * </summary>
     * <remarks>
     * The used port can differ depending on the <see cref="ServerSettings"/>. See also class description.
     * </remarks>
     */
    public int UsedPort => _usedPort;
    private int _usedPort = -1;

    /**
     * <summary>
     * Reports the protocol used by this server.
     * </summary>
     * <remarks>
     * The used protocol can differ depending on the <see cref="ServerSettings"/>. See also class description.
     * </remarks>
     * <example><c>"http"</c> or <c>"https"</c> or <c>"ws"</c> or <c>"wss"</c></example>
     */
    public string UsedProtocol => _usedProtocol;
    private string _usedProtocol = null;

    public BaseServer()
    {
        // Whether we should use SSL.
        // This is true if we are running in production mode or if the setting to use SSL only in production mode is
        // deactivated
        var secure = !ServerSettings.Instance.UseSslOnlyInProductionMode || !DebugSettings.Instance.DebugMode;

        // Whether to run a full HTTP server or websockets only
        // This is true if we are running in production mode or if the setting to use HTTP only in production mode is
        // deactivated
        var runHttpServer = !ServerSettings.Instance.RunHttpServerOnlyInProductionMode ||
                            !DebugSettings.Instance.DebugMode;

        // Determine the protocol depending on the above flags
        // E.g. http(s) or ws(s)
        _usedProtocol = runHttpServer ? (secure ? "https" : "http") : (secure ? "wss" : "ws");

        // Determine the port to use. If we are running in websocket-only mode, we use a different port so that the
        // replacement HTTP server for development can listen to the main port.
        _usedPort = runHttpServer ? ServerSettings.Instance.ControllerAppPort : ServerSettings.Instance.WebsocketPort;

        // Construct the server URL from the above flags
        // E.g. https://0.0.0.0:8000 or ws://0.0.0.0:4242
        _url = $"{_usedProtocol}://{Address}:{_usedPort}";

        // Construct websocket-only server, if running in websocket-only (debug) mode
        if (!runHttpServer)
        {
            Log(
                "Using web socket server only (because we are running in debug/development mode). Use a separate web server to server the controller software and a proxy to redirect websocket traffic here.");
            _wss = new WebSocketServer(_url);

            // If SSL shall be used, configure the server to use the certificate
            if (secure)
            {
                Log("Using SSL.");
                _wss.SslConfiguration.ServerCertificate = ServerSettings.Instance.RetrieveCertificate();
            }

            else
            {
                Log("Using *NO* SSL.");
            }
        }

        // Otherwise, construct a server which can serve documents over HTTP and websoockets...
        else
        {
            var documentRoot =
                $"{PathUtils.GetInstallDirectory()}/{ServerSettings.Instance.ControllerAppDocumentRoot}";

            Log(
                $"Serving websockets and web content on single port. Using the following document root: {documentRoot}");
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

            _httpServerBackend.OnGet += this.OnHTTPGet;
        }
    }

    public void Start()
    {
        Log($"Starting main server on {_url}...");
        _wss?.Start();
        _httpServerBackend?.Start();
    }

    public void Stop()
    {
        Log($"Stopping main server...");
        _wss?.Stop();
        _httpServerBackend?.Stop();
    }

    /**
     * <summary>
     * Register a websocket service.
     * 
     * Simply delegates to the websocket-sharp specific methods.
     * See websocket-sharp documentation for further information.
     * </summary>
     */
    public void AddWebSocketService<TBehaviorWithNew>(
        string path,
        Action<TBehaviorWithNew> initializer
    )
        where TBehaviorWithNew : WebSocketBehavior, new()
    {
        _wss?.AddWebSocketService(path, initializer);
        _httpServerBackend?.AddWebSocketService(path, initializer);
    }

    /**
     * <summary>
     * Register a websocket service.
     * 
     * Simply delegates to the websocket-sharp specific methods.
     * See websocket-sharp documentation for further information.
     * </summary>
     */
    public void AddWebSocketService<TBehaviorWithNew>(string path) where TBehaviorWithNew : WebSocketBehavior, new()
    {
        _wss?.AddWebSocketService<TBehaviorWithNew>(path);
        _httpServerBackend?.AddWebSocketService<TBehaviorWithNew>(path);
    }

    /**
     * <summary>
     * Get access to all websocket services created so far.
     * 
     * Simply delegates to the websocket-sharp specific methods.
     * See websocket-sharp documentation for further information.
     * </summary>
     */
    public WebSocketServiceManager WebSocketServices
    {
        get
        {
            if (_httpServerBackend != null)
            {
                return _httpServerBackend.WebSocketServices;
            }

            if (_wss != null)
            {
                return _wss.WebSocketServices;
            }

            throw new ApplicationException(
                "No websocket service provider available. This should never happen and is a programming error."
            );
        }
    }

    private static readonly HashSet<string> EncodingAnnotatedTypes = new HashSet<string> {"js", "css", "html", "htm"};

    /**
     * <summary>
     * Handles HTTP document GET requests by reading files and returning their content.
     * based on https://github.com/sta/websocket-sharp/blob/master/Example3/Program.cs
     * </summary>
     */
    private void OnHTTPGet(object sender, HttpRequestEventArgs eventArgs)
    {
        var req = eventArgs.Request;
        var res = eventArgs.Response;

        var path = req.RawUrl;

        // When a client tries to access /, deliver the index page instead
        if (path == "/")
        {
            path += "index.html";
        }

        // Try to read the contents of the requested file
        if (!eventArgs.TryReadFile(path, out var contents))
        {
            // Send 404 error, if there is no such file
            res.StatusCode = (int) HttpStatusCode.NotFound;
            
            LogWarning($"Client requested non existent file (404): {path}");
            
            return;
        }

        // Adjust the ContentType of response depending on the requested file
        var fileExtension = Path.GetExtension(path);
        if (fileExtension != null)
        {
            fileExtension = fileExtension.Remove(0, 1);
            if (MimeTypes.GetMimeType(fileExtension) is string mimeType)
            {
                res.ContentType = mimeType;
            }
            
            if (EncodingAnnotatedTypes.Contains(fileExtension))
            {
                res.ContentEncoding = Encoding.UTF8;
            }
        }
        
        res.ContentLength64 = contents.LongLength;
        res.Close(contents, true);
    }

    private static void Log(String str)
    {
        Debug.Log($"[BaseServer] {str}");
    }
    
    private static void LogWarning(String str)
    {
        Debug.LogWarning($"[BaseServer] {str}");
    }
}