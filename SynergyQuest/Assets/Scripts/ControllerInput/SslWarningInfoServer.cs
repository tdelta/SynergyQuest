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
 * Http server which is intended to serve a special web app over HTTP (no SSL) which informs users about certificate
 * warnings they might encounter when opening the main controller web app
 * (<see cref="ControllerServer"/> and <see cref="BaseServer"/>).
 *
 * This is a singleton, which manages startup and shutdown by itself.
 * </summary>
 * <remarks>
 * Since the game runs locally and uses web apis requiring encryption (Generic Sensor API), the main controller web app
 * is served with a self-signed certificate (see also <see cref="ServerSettings"/>).
 * This certificate causes warning messages in browsers.
 *
 * The idea of the "ssl-warning-info" web app which is intended to be served by this server is to inform users about
 * this warning and how to resolve it, before redirecting to the main controller web app.
 *
 * This server by default does not run in debug / non-production mode, but this can be changed in the settings.
 * Most settings are controlled by the <c>ServerSettings</c> scriptable object, e.g.
 *
 * * What port shall be used
 * * Where the document root for the web app is located
 * * ...
 *
 * The <see cref="LobbyMenuUi"/> displays the address of this server, instead of the main controller web app server,
 * when this server is running.
 * </remarks>
 */
public class SslWarningInfoServer: BehaviourSingleton<SslWarningInfoServer>
{
    [CanBeNull] private readonly HttpServer _httpServer;

    // Bind to all local addresses (0.0.0.0)
    private const string Address = "0.0.0.0";

    // Will store the full server URL after start depending on the <c>ServerSettings</c>
    // E.g. "https://0.0.0.0:8080"
    private string _url;

    /**
     * <summary>
     * Reports the port used by this server.
     * </summary>
     */
    public int UsedPort => ServerSettings.Instance.SslWarningInfoAppPort;

    /**
     * <summary>
     * Reports the protocol used by this server.
     * </summary>
     */
    public string UsedProtocol => "http";
    
    /**
     * <summary>
     * Makes sure, this singleton is initialized by the Unity runtime directly after `Awake` phase.
     * </summary>
     */
    [RuntimeInitializeOnLoadMethod]
    public static void EnsureInitialization()
    {
        var _ = SslWarningInfoServer.Instance;
    }

    private void Awake()
    {
        StartServer();
    }

    private void OnDestroy()
    {
        StopServer();
    }

    /**
     * <summary>
     * Reports, whether this server should be running.
     * The answer depends on the settings in <see cref="ServerSettings"/>.
     * </summary>
     */
    public bool MustBeUsed()
    {
        // If the ssl-warning-info web app shall only be served in production mode, then this server shall not
        // run if we are executing in debug mode.
        // If we are executing in production mode, it must run.
        return !DebugSettings.Instance.DebugMode | !ServerSettings.Instance.RunSslWarningInfoAppOnlyInProductionMode;
    }
    
    public SslWarningInfoServer()
    {
        if (MustBeUsed())
        {
            // Construct the server URL
            // E.g. http://0.0.0.0:8080
            _url = $"{UsedProtocol}://{Address}:{UsedPort}";

            var documentRoot =
                $"{PathUtils.GetInstallDirectory()}/{ServerSettings.Instance.SslWarningInfoDocumentRoot}";

            Log(
                $"Serving a web app over plain HTTP which informs users about the SSL warning they might encounter for the controller app. Serving the following dictionary: {documentRoot}"
            );
            _httpServer = new WebSocketSharp.Server.HttpServer(_url);

            if (DebugSettings.Instance.DebugMode)
            {
                _httpServer.Log.Level = LogLevel.Trace;
            }

            _httpServer.DocumentRootPath = documentRoot;

            _httpServer.OnGet += this.OnHTTPGet;
        }

        else
        {
            Log(
                $"Not serving the information web app regarding the SSL warning."
            );
            _httpServer = null;
        }
    }

    public void StartServer()
    {
        if (_httpServer != null)
        {
            Log($"Starting server for information about the SSL warning on {_url}...");
            _httpServer.Start();
        }
    }

    public void StopServer()
    {
        if (_httpServer != null)
        {
            Log($"Stopping server...");
            _httpServer.Stop();
        }
    }

    private static readonly HashSet<string> EncodingAnnotatedTypes = new HashSet<string> {"js", "css", "html", "htm"};

    /**
     * <summary>
     * Handles HTTP document GET requests by reading files and returning their content.
     * based on https://github.com/sta/websocket-sharp/blob/master/Example3/Program.cs
     * </summary>
     * <remarks>
     * FIXME: Duplicate code fragement with <see cref="BaseServer.OnHTTPGet"/>
     * </remarks>
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
        Debug.Log($"[SslWarningInfoServer] {str}");
    }
    
    private static void LogWarning(String str)
    {
        Debug.LogWarning($"[SslWarningInfoServer] {str}");
    }
}