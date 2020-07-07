using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/**
 * Implements a local HTTP server which servers the web based controller software.
 *
 * It searches for the controller software in the subfolder "controller-app" of the working directory.
 * Build the controller app with `npm run build` and ship the resulting contents in the `build` folder with the game
 * in a subfolder `controller-app`.
 *
 * To use it, just make sure this singleton is instantiated, e.g. by calling `EnsureInitialization`.
 * If a npm development server is already running on the server port, this server will just terminate with a warning.
 */
public class HttpServer: BehaviourSingleton<HttpServer>
{
    /**
     * Where the server will retrieve the web content it serves
     */
    private const string ControllerAppPath = "controller-app";
    /**
     * Port to bind to
     */
    private const short Port = 3000;
    
    /**
     * Indicates whether the server is running
     */
    private bool _running = false;
    /**
     * The server is running on a separate thread, hence we need some locking mechanisms
     */
    private readonly object _lock = new object();
    /**
     * Can be used to tell the worker thread to abort listening for a connection
     */
    private ManualResetEvent _stop;
    /**
     * The thread the server will run on
     */
    private Thread _thread;
    
    private HttpListener _listener;

    /**
     * Makes sure the server is started `OnAwake` in Unity by ensuring the server singleton is instantiated.
     */
    public static void EnsureInitialization()
    {
        var _ = HttpServer.Instance;
    }

    private void Awake()
    {
        Start();
    }

    private void OnDestroy()
    {
        Stop();
    }

    private void Start()
    {
        lock (_lock)
        {
            if (!_running && !(_thread?.IsAlive ?? false))
            {
                Debug.Log($"Starting HTTP server in {System.IO.Directory.GetCurrentDirectory()}");
                
                _stop = new ManualResetEvent(false);
                _running = true;
                _thread = new Thread(ThreadContent);
                
                _thread.Start();
            }
        }
    }

    private void Stop()
    {
        lock (_lock)
        {
            if (_running)
            {
                _running = false;
                _stop.Set();
                _thread.Join();
            }
        }
    }

    /**
     * Handles a client request
     */
    private void HandleRequest(IAsyncResult ar)
    {
        // Retrieve the client request
        var context = _listener.EndGetContext(ar);
        var response = context.Response; 
        
        // determine whicht page the client requested
        var page = context.Request.Url.LocalPath;
        // return index.html, if no specific page has been requested
        if (page == string.Empty || page == "/")
        {
            page = "/index.html";
        }
        page = ControllerAppPath + page;

        try
        {
            // Read the file the client wants
            var fileReader = new StreamReader(page);
            var fileContent = fileReader.ReadToEnd(); //getting the page's content

            // write page content into buffer to send to client
            var buffer = Encoding.UTF8.GetBytes(fileContent);

            // Stream content to the client
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        catch (Exception e)
        {
            Debug.LogException(e);
        }

        finally
        {
            // Close connection to client
            context.Response.Close();
        }
    }
    
    private void ThreadContent()
    {
        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{Port}/");
            _listener.Start();
        }

        catch (SocketException e)
        {
            Debug.LogWarning(
                "Could not start the internal HTTP server since some other application is using the port. Are you running the npm development server? In that case, this is fine."
            );
            
            _listener.Stop();
            _running = false;
            return;
        }
        
        while (_running)
        {
            // Start listening for a connection
            var context = _listener.BeginGetContext(
                HandleRequest, // call this method to handle the connection
                null // pass no additional information to the handler
            );

            // Wait on both, the _stop event and the connection listener.
            // If the _stop event is resolved (index == 0), stop the server by breaking the loop
            if (0 == WaitHandle.WaitAny(new[] {_stop, context.AsyncWaitHandle}))
            {
                break;
            }
        }
        
        _listener.Stop();
    }
}
