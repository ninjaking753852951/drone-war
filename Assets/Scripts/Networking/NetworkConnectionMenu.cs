using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtils;

public class NetworkConnectionMenu : MonoBehaviour
{
    NetworkManager net;
    string ipAddress = "127.0.0.1"; // Default IP
    const string IP_PREFS_KEY = "SavedIPAddress";

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

    void StartClient()
    {
        net.StartClient();
        Destroy(this);
    }

    void StartHost()
    {
        net.StartHost();
        Destroy(this);
    }

    void SetIP(string ip)
    {
        UnityTransport transport = GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ip;

        // Save the IP address to PlayerPrefs
        PlayerPrefs.SetString(IP_PREFS_KEY, ip);
        PlayerPrefs.Save();
    }

    void OnGUI()
    {
        if (GameManager.Instance.currentGameMode != GameMode.Battle)
            return;

        // Get the center of the screen
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Set button dimensions
        float buttonWidth = 120;
        float buttonHeight = 40;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.red }
        };

        GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = 20
        };

        // Position for the Host button
        Rect hostButtonRect = new Rect(
            (screenWidth - buttonWidth) / 2,
            100, // Position slightly below the top
            buttonWidth,
            buttonHeight
        );

        // Position for the Join button
        Rect joinButtonRect = new Rect(
            (screenWidth - buttonWidth) / 2,
            160,
            buttonWidth,
            buttonHeight
        );

        // Create Host button
        if (GUI.Button(hostButtonRect, "Host", buttonStyle))
        {
            StartHost();
        }

        // Create Join button
        if (GUI.Button(joinButtonRect, "Join", buttonStyle))
        {
            SetIP(ipAddress); // Set the IP before starting the client
            StartClient();
        }

        // Input field for the IP address
        Rect inputFieldRect = new Rect(
            joinButtonRect.xMax + 20, // Positioned to the right of the Join button
            joinButtonRect.y,
            200, // Width of the input field
            buttonHeight
        );
        ipAddress = GUI.TextField(inputFieldRect, ipAddress, textFieldStyle);

        // Dismiss button to disable the menu
        Rect dismissButtonRect = new Rect(
            hostButtonRect.xMax + 20, // Positioned to the right of the Host button
            hostButtonRect.y,
            buttonWidth,
            buttonHeight
        );
        if (GUI.Button(dismissButtonRect, "Dismiss", buttonStyle))
        {
            gameObject.SetActive(false);
        }
    }
}
