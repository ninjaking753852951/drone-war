using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtils;

public class NetworkConnectionMenu : MonoBehaviour
{
    NetworkManager net;
    public string ipAddress = "127.0.0.1"; // Default IP
    const string IP_PREFS_KEY = "SavedIPAddress";

    public bool showOverride = false;

    void Awake()
    {
        net = GetComponent<NetworkManager>();

        // Load the saved IP address if it exists
        if (PlayerPrefs.HasKey(IP_PREFS_KEY))
        {
            ipAddress = PlayerPrefs.GetString(IP_PREFS_KEY);
        }
    }

    void Start()
    {
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
    }

    public void StartClient()
    {
        net.StartClient();
    }

    public void StartHost()
    {
        net.StartHost();
    }
    
    public void Disconnect()
    {
        net.Shutdown();
    }

    public void SetIP(string ip)
    {
        UnityTransport transport = GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ip;

        // Save the IP address to PlayerPrefs
        PlayerPrefs.SetString(IP_PREFS_KEY, ip);
        PlayerPrefs.Save();
    }
}
