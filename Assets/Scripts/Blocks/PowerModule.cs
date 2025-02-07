using UnityEngine;
public class PowerModule : MonoBehaviour
{

     float powerCapacity = 0;
     float powerRegen = 0;

    DroneController controller;
    
    public void Init()
    {
        controller = transform.root.GetComponentInChildren<DroneController>();

        DroneBlock block = GetComponent<DroneBlock>();

        powerCapacity = block.stats.QueryStat(Stat.EnergyCapacity);
        powerRegen = block.stats.QueryStat(Stat.EnergyRegen);
        
        controller.energy.maxEnergy += powerCapacity;
        controller.energy.energyRegenRate += powerRegen;
    }
}
