using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityUtils;

public class WorldUIManager : Singleton<WorldUIManager>
{
    public Camera mainCamera; // Reference to the main camera
    public Canvas canvas; // Reference to the UI Canvas

    public HealthBarManager healthBarManager;
    public IconManager iconManager;
    
    [Serializable]
    public class HealthBarManager
    {
        public GameObject healthBarPrefab; // The prefab for the health bar

        public List<HealthBars> healthBars = new List<HealthBars>();

        WorldUIManager ui;
        
        public void Init(WorldUIManager ui)
        {
            this.ui = ui;
        }
        
        public void Update()
        {
            UpdateHealthBarPositions();

            foreach (var healthBar in healthBars)
            {
                healthBar.UpdateFill();
            }
        }
        
        public void RegisterHealthBar(IProgressBar drone)
        {
            GameObject healthBar = Instantiate(healthBarPrefab, ui.canvas.transform);
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
                    Vector3 screenPosition = ui.mainCamera.WorldToScreenPoint(pos);
                    Debug.Log(ui.mainCamera.transform.position);
                    screenPosition.y += healthbar.settings.screenOffsetHeight;
                    healthbar.obj.transform.position = screenPosition;
                }
            }
        }
        
    }
    
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
    
    [Serializable]
    public class IconManager
    {
        public GameObject iconPrefab;

        List<WorldUIIcon> worldUIIcons;

        public WorldUIManager ui { get; set; }

        public void Init(WorldUIManager ui)
        {
            this.ui = ui;
            worldUIIcons = new List<WorldUIIcon>();
        }
        
        public WorldUIIcon RegisterIcon(Transform target, WorldUIIconFactory iconFactory)
        {
            WorldUIIcon icon = iconFactory.Create(this, target); 
            worldUIIcons.Add(icon);
            return icon;
        }

        public void UnregisterIcon(WorldUIIcon icon)
        {
            icon.Dispose();
            worldUIIcons.Remove(icon);
        }

        public void Update()
        {
            UpdateIconPositions();
        }
        
        private void UpdateIconPositions()
        {
            foreach (var icon in worldUIIcons)
            {
                if (icon.target == null)
                {
                    UnregisterIcon(icon);
                    break;
                }
                
                Vector3 pos = icon.target.position;
                pos.y += icon.worldUIIconFactory.worldOffset.y;
                if (icon.obj != null)
                {
                    Vector3 screenPosition = ui.mainCamera.WorldToScreenPoint(pos);
                    screenPosition.y += icon.worldUIIconFactory.screenOffset.y;
                    icon.obj.transform.position = screenPosition;
                }
            }
        }

    }
    
    
    public class WorldUIIcon
    {
        public WorldUIIconFactory worldUIIconFactory;
        public GameObject obj;
        public Transform target;
        public WorldUIIcon(WorldUIIconFactory worldUIIconFactory, GameObject obj, Transform target)
        {
            this.worldUIIconFactory = worldUIIconFactory;
            this.obj = obj;
            this.target = target;
        }

        public void Dispose()
        {
            Destroy(obj);
        }
    }

    void Start()
    {
        healthBarManager.Init(this);
        iconManager.Init(this);
    }

    void Update()
    {
        healthBarManager.Update();
        iconManager.Update();
    }


}
