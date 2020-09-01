using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

/**
 * <summary>
 * The game must serve multiple network services for the remote controllers. They are configured by this scriptable
 * object singleton, see also <see cref="ScriptableObjectSingleton{T}"/>.
 * The services include
 * <list type="bullet">
 *     <item><description>A HTTP(S) server serving the controller web app (production only, in debug mode, an external web server suited for debugging should be used).</description></item>
 *     <item><description>A websocket service which can be used to communicate controller input and settings</description></item>
 *     <item><description>A websocket service which can be used to communicate diagnostic information about the game state.</description></item>
 * </list>
 * </summary>
 * <seealso cref="ScriptableObjectSingleton{T}"/>
 * <seealso cref="BaseServer"/>
 * <seealso cref="ControllerServer"/>
 * <seealso cref="ControllerConnection"/>
 * <seealso cref="DiagnosticsConnection"/>.
 */
[CreateAssetMenu(fileName = "ServerSettings", menuName = "ScriptableObjects/ServerSettings")]
public class ServerSettings: ScriptableObjectSingleton<ServerSettings>
{
    /**
     * <summary>
     * Port on which the HTTP server shall be served.
     * </summary>
     * <seealso cref="BaseServer"/>
     */
    public int HttpPort => httpPort;
    [SerializeField]
    private int httpPort = 8000;

    /**
     * <summary>
     * Port on which the websocket services shall be served.
     * In production mode, the websockets are normally also served by the HTTP server, in which case this setting is
     * ignored.
     * </summary>
     * <seealso cref="BaseServer"/>
     * <seealso cref="runHttpServerOnlyInProductionMode"/>
     */
    public int WebsocketPort => websocketPort;
    [SerializeField] private int websocketPort = 4242;
    
    /**
     * <summary>
     * Path of the web-based controller app which is served by `BaseServer` in production mode.
     * Build the controller app with `yarn build` and ship the resulting contents with the game at the path given here.
     * </summary>
     * <seealso cref="BaseServer"/>
     * <seealso cref="RunHttpServerOnlyInProductionMode"/>
     */
    public string DocumentRoot => documentRoot;
    [SerializeField] private string documentRoot = "controller-app";
    
    /**
     * <summary>
     * When being shipped, the game of course has to serve the controller web app itself.
     * However, during debugging, it is oftentimes advisable to use an external web server suited to this task, e.g.
     * the one provided by <c>react-scripts</c>.
     *
     * Hence, by default, the game only runs the HTTP server in production mode. However, iff this setting is set to
     * false, it is also run in debug mode.
     * </summary>
     * <seealso cref="DebugSettings.DebugMode"/>
     * <seealso cref="BaseServer"/>
     */
    public bool RunHttpServerOnlyInProductionMode => runHttpServerOnlyInProductionMode;
    [SerializeField] private bool runHttpServerOnlyInProductionMode = true;

    /**
     * <summary>
     * The remote controllers require the web app to be served over SSL encrypted connections.
     * This is because modern browsers only provide some sensor inputs in secure contexts, see also
     * <a href="https://developer.mozilla.org/en-US/docs/Web/Security/Secure_Contexts/features_restricted_to_secure_contexts">Link</a>.
     *
     * However, since in debug mode usually an external web server is used to serve the controller app,
     * <see cref="RunHttpServerOnlyInProductionMode"/>, we do not want to use SSL in debug mode
     * (<see cref="DebugSettings.DebugMode"/>).
     * Instead, during debugging, a proxy server should be used (<c>Controller/proxy</c>) which takes care of SSL
     * encryption and delegates HTTP traffic to the external webserver and Websocket traffic to the game server.
     * Hence, SSL is deactivated by default in debug mode, but this is overridden, iff this field is set to false.
     * </summary>
     * <seealso cref="DebugSettings.DebugMode"/>
     * <seealso cref="BaseServer"/>
     * <seealso cref="RetrieveCertificate"/>
     */
    public bool UseSslOnlyInProductionMode => useSSLOnlyInProductionMode;
    [SerializeField] private bool useSSLOnlyInProductionMode = true;
    
    /**
     * <summary>
     * When encrypting connections to the game's server, a certificate is required.
     * It can be acquired using this method.
     *
     * Set the <see cref="pathToCertificatePFX"/> and <see cref="pfxPassword"/> settings to configure which certificate
     * is used.
     * 
     * For now, the game by default ships with a self-signed, pre-generated certificate file with an extra long expiry
     * date (~900 years), since the game runs in the local network.
     * This is a bit unfortunate though, since users are greeted with a warning about the certificate when first opening
     * the app. The certificate can be generated using the script <c>Certificates/generate-self-signed.sh</c>.
     *
     * There are also some ways to let the game generate a self-signed certificate instead of using a certificate with a
     * high expiry date, but they have drawbacks:
     *
     * <list type="bullet">
     *  <item><description>requires dependency on BouncyCastle library: <a href="https://stackoverflow.com/a/9250034">Link</a></description></item>
     *  <item>
     *    <description>
     *      requires .NET features, which seem not to be accessible in the current Unity version:
     *      <a href="https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin">Link</a>
     *    </description>
     *  </item>
     * </list>
     * </summary>
     * <seealso cref="BaseServer"/>
     * <seealso cref="UseSslOnlyInProductionMode"/>
     */
    public X509Certificate2 RetrieveCertificate()
    {
        // FIXME: Eventually this path has to be adjusted to use backslashes in windows systems!
        #if UNITY_EDITOR
        var pathToCertificatePfx = pathToCertificatePfxForEditorMode;
        #else
        var pathToCertificatePfx = pathToCertificatePfxForBuildMode;
        #endif
        pathToCertificatePfx = $"{PathUtils.GetInstallDirectory()}/{pathToCertificatePfx}";
     
        if (File.Exists(pathToCertificatePfx))
        {
            return new X509Certificate2 (pathToCertificatePfx, pfxPassword);
        }

        else
        {
            throw new ApplicationException($"Could not find certificate .pfx file at {pathToCertificatePfx}.");
        }
    }
    
    /**
     * Certificates must be provided as a PFX file.
     * This path will be used to locate the file when running in editor mode.
     */
    [SerializeField] private string pathToCertificatePfxForEditorMode = "../Certificates/generated/server.pfx";
    /**
     * Certificates must be provided as a PFX file.
     * This path will be used to locate the file when running in the Unity Player (Release Build).
     */
    [SerializeField] private string pathToCertificatePfxForBuildMode = "Certificates/server.pfx";
    [SerializeField] private string pfxPassword = "";
}