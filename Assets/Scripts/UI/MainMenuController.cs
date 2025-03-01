using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{

    public Button buildModeButton;
    public Button battleModeButton;
    public Button settingsMenuButton;
    public Button quitGameButton;

    SettingsMenuController settingsMenuController;

    void Awake()
    {
        settingsMenuController = FindFirstObjectByType<SettingsMenuController>();
        
        settingsMenuButton.onClick.AddListener(settingsMenuController.OpenMenu);
        buildModeButton.onClick.AddListener(GameManager.Instance.GoToBuildMode);
        battleModeButton.onClick.AddListener(GameManager.Instance.GoToBattleMode);
        quitGameButton.onClick.AddListener(Application.Quit);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
