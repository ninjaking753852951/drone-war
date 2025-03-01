using ImprovedTimers;
using UnityEngine;
[System.Serializable]
public class EnergyController : IProgressBar
{
    [HideInInspector]
    public float energy;
    public float maxEnergy;
    public float energyRegenRate;

    public WorldUIIconFactory lowPowerIcon;
    WorldUIManager.WorldUIIcon curLowPowerIcon;
    Timer outOfPowerCooldown = new CountdownTimer(0);
    
    
    public ProgressBarSettings energyBarSettings;
        
    DroneController controller;

    public void Init(DroneController controller)
    {
        this.controller = controller;
        energy = maxEnergy;
        WorldUIManager.Instance.healthBarManager.RegisterHealthBar(this);
    }

    public void Update(float timeDelta)
    {
        energy = Mathf.MoveTowards(energy, maxEnergy, energyRegenRate * timeDelta);
    
        
        if (outOfPowerCooldown.IsFinished && curLowPowerIcon != null)
        {
            WorldUIManager.Instance.iconManager.UnregisterIcon(curLowPowerIcon);
            curLowPowerIcon = null;
        }
    }
    public Transform ProgressBarWorldTarget()
    {
        return controller.transform;
    }
    public float ProgressBarFill()
    {
        return energy / maxEnergy;
    }
    public float ProgressBarMaximum()
    {
        return maxEnergy;
    }
    public ProgressBarSettings ProgressBarSettings()
    {
        return energyBarSettings;
    }
    public bool IsDestroyed()
    {
        return controller == null;
    }

    public bool DeductEnergy(float amount)
    {
        if (energy - amount >= 0)
        {
            energy -= amount;
            return true;
        }
        else
        {
            Debug.Log("out of power");
            outOfPowerCooldown = new CountdownTimer(0.5f);
            outOfPowerCooldown.Start();
            
            if (curLowPowerIcon == null)
            {
                curLowPowerIcon = WorldUIManager.Instance.iconManager.RegisterIcon(this.controller.transform, lowPowerIcon);   
            }
            return false;
        }
    }

    public bool CanAfford(float amount) => energy - amount >= 0;
}