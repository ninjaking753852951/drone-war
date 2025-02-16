using System;
using System.Collections.Generic;
using System.IO;
using Settings;
using UnityEngine;

[System.Serializable]
public class GameSettings
{
    public float cameraRotationSpeed = 1;
    public UIScalingProperty uiScaling;
    public CameraRotationSpeedProperty cameraRotationSpeedProperty;
    public VolumeSettingProperty volumeSettingProperty;
    public QualitySettingProperty qualitySettingProperty;
    
    public bool clearSettingsFile;
    
    public GameSettings()
    {
        uiScaling = new UIScalingProperty();
        cameraRotationSpeedProperty = new CameraRotationSpeedProperty();
        volumeSettingProperty = new VolumeSettingProperty();
        qualitySettingProperty = new QualitySettingProperty();
    }
    public void ApplySettings()
    {
        qualitySettingProperty.ApplySetting();
        volumeSettingProperty.ApplySetting();
        uiScaling.ApplySetting();
        cameraRotationSpeedProperty.ApplySetting();
    }

    
    public static GameSettings Load(string applicationPath, bool clear)
    {
        
        string SettingsFilePath = Path.Combine(applicationPath, "config.json");
        
        EnsureDirectoryExists();

        if (File.Exists(SettingsFilePath) && !clear)
        {
            //Debug.Log("loading found Settings");
            string json = File.ReadAllText(SettingsFilePath);
            return JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            //Debug.Log("loading fresh Settings");
            var defaultSettings = new GameSettings();
            defaultSettings.Save();
            return defaultSettings;
        }
    }

    // Save the current settings to file
    public void Save()
    {
        //Debug.Log("Saving Settings");
        string SettingsFilePath = Path.Combine(Application.persistentDataPath, "config.json");
        
        string json = JsonUtility.ToJson(this, prettyPrint: true);
        File.WriteAllText(SettingsFilePath, json);
    }

    // Ensure the directory for the settings file exists
    private static void EnsureDirectoryExists()
    {
        string SettingsFilePath = Path.Combine(Application.persistentDataPath, "config.json");
        
        string directoryPath = Path.GetDirectoryName(SettingsFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
