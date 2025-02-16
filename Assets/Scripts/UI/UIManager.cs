using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityUtils;
public class UIManager : Singleton<UIManager>
{

    EscapeMenuController escapeMenu;
    SettingsMenuController settingsMenu;
    CanvasScaler canvasScaler;

    public ConfirmationBox confirmationBox;
    
    public bool IsOpen() => escapeMenu.IsOpen() || settingsMenu.IsOpen();

    void Awake()
    {
        base.Awake();

        canvasScaler = GetComponent<CanvasScaler>();
        escapeMenu = GetComponentInChildren<EscapeMenuController>();
        settingsMenu = GetComponentInChildren<SettingsMenuController>();
        CloseMenus();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!IsOpen())
                OpenMenu();
            else
                CloseMenus();    
            
        }
    }

    void OpenMenu()
    {
        escapeMenu.OpenMenu();
    }
    
    void CloseMenus()
    {
        settingsMenu.CloseMenu();
        escapeMenu.CloseMenu();
    }

    public void SetUIScaling(int value)
    {
        canvasScaler.scaleFactor = UIScaleConverter(value);
    }
    
    float UIScaleConverter(int value)
    {
        switch (value)
        {
            case 0: // 1x
                return 1;
                break;
            case 1: // 1.5x
                return 1.5f;
                break;
            case 2: // 2x
                return 2;
                break;
        }
        return 1;
    }
}
