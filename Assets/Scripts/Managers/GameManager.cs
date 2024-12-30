using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityUtils;

public class GameManager : PersistentSingleton<GameManager>
{

    public GameMode currentGameMode;

    public UnityEvent onExitBuildMode;
    public UnityEvent onEnterBuildMode;

    public bool gameModeMenuIsOpen;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gameModeMenuIsOpen = !gameModeMenuIsOpen;
    }

    public bool IsOnlineAndClient()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            return !NetworkManager.Singleton.IsServer;
        }
        else
        {
            return false;
        }
    }

    public void ExitBuildMode()
    {
        currentGameMode = GameMode.TestDrive;
        onExitBuildMode.Invoke();
    }

    public void EnterBuildMode()
    {
        currentGameMode = GameMode.Build;
        onEnterBuildMode.Invoke();
    }
    
    void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void OnGUI()
    {
        if(!gameModeMenuIsOpen)
            return;
            
        // Define button styles (optional customization)
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };

        // Define button dimensions
        float buttonWidth = 100;
        float buttonHeight = 30;
        float padding = 20; // Spacing between buttons

        // Calculate total width of the button group (two buttons + padding)
        float totalWidth = (2 * buttonWidth) + padding;

        // Calculate positions for the buttons
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float startX = (screenWidth - totalWidth) / 2; // Center the buttons horizontally
        float startY = screenHeight/2; // Distance from the top of the screen

        // Button positions
        Rect buildButtonRect = new Rect(startX, startY, buttonWidth, buttonHeight);
        Rect battleButtonRect = new Rect(startX + buttonWidth + padding, startY, buttonWidth, buttonHeight);

        // "Build" button
        if (GUI.Button(buildButtonRect, "Build", buttonStyle))
        {
            SwapGameMode(GameMode.Build);
        }

        // "Battle" button
        if (GUI.Button(battleButtonRect, "Battle", buttonStyle))
        {
            SwapGameMode(GameMode.Battle);
        }
    }

    void SwapGameMode(GameMode newGameMode)
    {
        gameModeMenuIsOpen = false;
        currentGameMode = newGameMode;
        ReloadScene();
    }
}

public enum GameMode
{
    Build, TestDrive, Battle
}
