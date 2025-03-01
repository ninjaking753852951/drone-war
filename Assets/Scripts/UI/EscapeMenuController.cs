using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscapeMenuController : MonoBehaviour
{

    public GameObject menuParent;
    public Button openSettingsButton;
    public Button backButton;
    public Button mainMenuButton;
    public Button quitButton;

    SettingsMenuController settingsMenu;
    
    public bool IsOpen() => menuParent.activeSelf;

    void Awake()
    {
        backButton.onClick.AddListener(CloseMenu);
        settingsMenu = FindFirstObjectByType<SettingsMenuController>();
        openSettingsButton.onClick.AddListener(settingsMenu.OpenMenu);
        openSettingsButton.onClick.AddListener(CloseMenu);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(Application.Quit);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenMenu()
    {
        menuParent.SetActive(true);   
    }

    public void CloseMenu()
    {
        menuParent.SetActive(false);   
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
