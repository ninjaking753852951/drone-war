using UnityEngine;
[System.Serializable]
public class EnergyController : IProgressBar
{
    [HideInInspector]
    public float energy;
    public float maxEnergy;
    public float energyRegenRate;

    public ProgressBarSettings energyBarSettings;
        
    DroneController controller;

    public void Init(DroneController controller)
    {
        this.controller = controller;
        energy = maxEnergy;
        ProgressBarManager.Instance.RegisterHealthBar(this);
    }

    public void Update(float timeDelta)
    {
        energy = Mathf.MoveTowards(energy, maxEnergy, energyRegenRate * timeDelta);
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
            return false;
        }
    }

    public bool CanAfford(float amount) => energy - amount >= 0;
}