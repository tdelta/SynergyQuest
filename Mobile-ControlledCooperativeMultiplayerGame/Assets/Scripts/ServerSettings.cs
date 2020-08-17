using System.IO;
using System.Security.Cryptography.X509Certificates;
using Boo.Lang.Runtime;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerSettings", menuName = "ScriptableObjects/ServerSettings")]
public class ServerSettings: ScriptableObjectSingleton<ServerSettings>
{
    // For now we use a self-signed, pre-generated certificate file with an extra long expiry date (~900 years)
    // since this is just to fool web browsers since they require SSL encrypted connections to access IMU sensors.
    //
    // We do not transmit any sensitive data in this game anyway.
    //
    // However, there are also some ways to let the game generate a self-signed certificate, but they have drawbacks:
    // 
    // * requires dependency on BouncyCastle library: https://stackoverflow.com/a/9250034
    // * requires .NET features, which seem not to be accessible in the current Unity version:
    //   https://stackoverflow.com/questions/42786986/how-to-create-a-valid-self-signed-x509certificate2-programmatically-not-loadin

    // FIXME: This path will not be accessible in the packaged game. Make the certificate a packageable resource
    [SerializeField] private string pathToCertificatePFX = "../Certificates/generated/server.pfx";
    [SerializeField] private string pfxPassword = "";

    [SerializeField] private string documentRoot = "controller-app";
    public string DocumentRoot => documentRoot;
    
    [SerializeField]
    private float httpPort = 8000;
    public float HttpPort => httpPort;

    [SerializeField] private float websocketPort = 4242;
    public float WebsocketPort => websocketPort;

    [SerializeField] private bool runHttpServerOnlyInProductionMode = true;
    public bool RunHttpServerOnlyInProductionMode => runHttpServerOnlyInProductionMode;

    [SerializeField] private bool useSSLOnlyInProductionMode = true;
    public bool UseSslOnlyInProductionMode => useSSLOnlyInProductionMode;

    public X509Certificate2 RetrieveCertificate()
    {
        if (File.Exists(pathToCertificatePFX))
        {
            return new X509Certificate2 (pathToCertificatePFX, pfxPassword);
        }

        else
        {
            throw new RuntimeException($"Could not find certificate .pfx file at {pathToCertificatePFX}.");
        }
    }
}