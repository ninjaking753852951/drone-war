using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityUtils;

public class GameManager : PersistentSingleton<GameManager>
{

    public GameMode currentGameMode;

    public UnityEvent onExitBuildMode;
    public UnityEvent onEnterBuildMode;

    public bool gameModeMenuIsOpen;
    
    public GameSettings gameSettings;

    
    protected override void Awake()
    {
        base.Awake();
        gameSettings = GameSettings.Load(Application.persistentDataPath, gameSettings.clearSettingsFile);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        gameSettings.ApplySettings();
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SwapGameMode(GameMode newGameMode)
    {
        gameModeMenuIsOpen = false;
        currentGameMode = newGameMode;
        ReloadScene();
    }

    /*public void SwitchToBuildMode()
    {
        SwapGameMode(GameMode.Build);
    }

    public void SwitchToBattleMode()
    {
        SwapGameMode(GameMode.Battle);
    }*/

    public void GoToBuildMode()
    {
        SceneManager.LoadScene("BuildScene");
        Instance.currentGameMode = GameMode.Build;
    }

    public void GoToBattleMode()
    {
        SceneManager.LoadScene("BattleScene");
        Instance.currentGameMode = GameMode.Battle;
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void OnDisable()
    {
        gameSettings.Save();
    }
}

public enum GameMode
{
    Build, TestDrive, Battle
}
