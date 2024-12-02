using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityUtils;

public class HealthBarManager : Singleton<HealthBarManager>
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
        public DroneController controller;

        public HealthBars(DroneController controller, GameObject obj)
        {
            this.target = controller.transform;
            this.slider = obj.GetComponentInChildren<Slider>();
            this.obj = obj;
            this.controller = controller;
        }

        public void UpdateFill()
        {
            slider.value = controller.curHealth / controller.maxHealth;
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

    public void RegisterHealthBar(DroneController drone)
    {
        GameObject healthBar = Instantiate(healthBarPrefab, canvas.transform);
        healthBars.Add(new HealthBars(drone, healthBar));
    }

    public void UnregisterHealthBar(DroneController worldObject)
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
            if (healthbar.controller == null)
            {
                UnregisterHealthBar(healthbar.controller);
                break;
            }
            Vector3 pos = healthbar.controller.transform.position;
            pos.y += 5;
            if (healthbar.obj != null)
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(pos);
                healthbar.obj.transform.position = screenPosition;
               // Debug.Log("HEALTHBAR");
            }

        }
    }


}
