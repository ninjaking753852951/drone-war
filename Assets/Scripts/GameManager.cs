using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityUtils;

public class GameManager : PersistentSingleton<GameManager>
{

    public GameMode currentGameMode;

    public UnityEvent onExitBuildMode;
    public UnityEvent onEnterBuildMode;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        // Define button styles (optional customization)
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };

        // Define button dimensions
        float buttonWidth = 50;
        float buttonHeight = 20;
        float padding = 2;

        // Calculate positions for the buttons
        Rect buildButtonRect = new Rect(padding, padding, buttonWidth, buttonHeight);
        Rect battleButtonRect = new Rect(padding + buttonWidth + padding, padding, buttonWidth, buttonHeight);

        // "Build" button
        if (GUI.Button(buildButtonRect, "Build", buttonStyle))
        {
            currentGameMode = GameMode.Build;
            ReloadScene();
        }

        // "Battle" button
        if (GUI.Button(battleButtonRect, "Battle", buttonStyle))
        {
            currentGameMode = GameMode.Battle;
            ReloadScene();
        }
    }
}

public enum GameMode
{
    Build, TestDrive, Battle
}
