using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityUtils;

public class NetworkConnectionMenu : MonoBehaviour
{

    NetworkManager net;

    void Awake()
    {
        net = GetComponent<NetworkManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
    }

    // Update is called once per frame
    void Update()
    {
        
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
    
    //TODO create a host and join buttons 50 px down from the top middle of the screen 
    void OnGUI()
    {
        if(GameManager.Instance.currentGameMode != GameMode.Battle)
            return;
        
        // Get the center of the screen
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Set button dimensions
        float buttonWidth = 120;
        float buttonHeight = 40;
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 30; // Larger font size
        buttonStyle.fontStyle = FontStyle.Bold; // Bold text
        buttonStyle.normal.textColor = Color.red; // Text color

        // Position for the Host button
        Rect hostButtonRect = new Rect(
            (screenWidth - buttonWidth) / 2, // Center horizontally
            50 + 50, // 50px down from the top
            buttonWidth,
            buttonHeight
        );

        // Position for the Join button
        Rect joinButtonRect = new Rect(
            (screenWidth - buttonWidth) / 2, // Center horizontally
            110 + 50, // 50px down + button height + spacing
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
            StartClient();
        }
    }
}
