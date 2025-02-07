using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityUtils;

public class ProgressBarManager : Singleton<ProgressBarManager>
{
    public Camera mainCamera; // Reference to the main camera
    public Canvas canvas; // Reference to the UI Canvas
    public GameObject healthBarPrefab; // The prefab for the health bar

    public List<HealthBars> healthBars = new List<HealthBars>();

    [System.Serializable]
    public class HealthBars
    {
        public Transform target;
        public GameObject obj;
        Slider slider; 
        public IProgressBar controller;
        public ProgressBarSettings settings;

        public float fill;

        public HealthBars(IProgressBar controller, GameObject obj)
        {
            this.target = controller.ProgressBarWorldTarget();
            this.slider = obj.GetComponentInChildren<Slider>();

            HealthbarController healthbarController = obj.GetComponent<HealthbarController>();
            if(healthbarController != null)
                healthbarController.GenerateHealthBar(controller.ProgressBarMaximum());
            
            obj.transform.FindChildWithTag("UIIcon").GetComponent<Image>().color = controller.ProgressBarSettings().colour;
            this.obj = obj;
            this.controller = controller;
            settings = this.controller.ProgressBarSettings();
        }

        public void UpdateFill()
        {
            float newFill = controller.ProgressBarFill();
            if(!float.IsNaN(newFill))
                fill = newFill;
            
            slider.value = Mathf.Clamp01(fill);
        }
    }
    
    void Update()
    {
        UpdateHealthBarPositions();

        foreach (var healthBar in healthBars)
        {
            healthBar.UpdateFill();
        }
    }

    public void RegisterHealthBar(IProgressBar drone)
    {
        GameObject healthBar = Instantiate(healthBarPrefab, canvas.transform);
        healthBars.Add(new HealthBars(drone, healthBar));
    }

    public void UnregisterHealthBar(IProgressBar worldObject)
    {
        foreach (var healthBar in healthBars)
        {
            if (healthBar.controller == worldObject)
            {
                Destroy(healthBar.obj);
                healthBars.Remove(healthBar);
                break;
            }
        }
    }

    private void UpdateHealthBarPositions()
    {
        
        foreach (var healthbar in healthBars)
        {
            if (healthbar.controller.IsDestroyed())
            {
                UnregisterHealthBar(healthbar.controller);
                break;
            }
            Vector3 pos = healthbar.controller.ProgressBarWorldTarget().position;
            pos.y += healthbar.settings.worldOffsetHeight;
            if (healthbar.obj != null)
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(pos);
                screenPosition.y += healthbar.settings.screenOffsetHeight;
                healthbar.obj.transform.position = screenPosition;
               // Debug.Log("HEALTHBAR");
            }

        }
    }


}
