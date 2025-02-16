using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{

    public GameObject menuParent;
    
    [FormerlySerializedAs("uiScallingDropdown")]
    public TMP_Dropdown uiScalingDropdown;
    public TMP_Dropdown graphicsQualityDropdown;
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider cameraRotationSlider;
    public Button backButton;
    
    Canvas canvas;
    CanvasScaler canvasScaler;

    public bool IsOpen() => menuParent.activeSelf;
    
    void Awake()
    {
        backButton.onClick.AddListener(CloseMenu);
        canvas = GetComponentInParent<Canvas>();
        canvasScaler = canvas.gameObject.GetComponent<CanvasScaler>();

        
        //sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiScalingDropdown.onValueChanged.AddListener(SetUIScaling);
        uiScalingDropdown.value = GameManager.Instance.gameSettings.uiScaling.value;
        
        cameraRotationSlider.onValueChanged.AddListener(GameManager.Instance.gameSettings.cameraRotationSpeedProperty.SetValue);
        cameraRotationSlider.value = GameManager.Instance.gameSettings.cameraRotationSpeedProperty.value;
        
        masterVolumeSlider.onValueChanged.AddListener(GameManager.Instance.gameSettings.volumeSettingProperty.SetValue);
        masterVolumeSlider.value = GameManager.Instance.gameSettings.volumeSettingProperty.value;

        graphicsQualityDropdown.onValueChanged.AddListener(GameManager.Instance.gameSettings.qualitySettingProperty.SetValue);
        graphicsQualityDropdown.value = GameManager.Instance.gameSettings.qualitySettingProperty.value;

        //SetUIScaling(GameManager.Instance.gameSettings.uiScaling);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetUIScaling(int x)
    {
        GameManager.Instance.gameSettings.uiScaling.SetValue(x);
    }
    
    void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level);
    }
    
    public void OpenMenu()
    {
        menuParent.SetActive(true);   
    }

    public void CloseMenu()
    {
        menuParent.SetActive(false);   
    }

    public void SetMasterVolume(float volume)
    {
        SFXManager.Instance.SetMasterVolume(volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        SFXManager.Instance.SetSFXVolume(volume);
    }
}
