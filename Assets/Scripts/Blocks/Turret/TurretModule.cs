using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TurretModule : MonoBehaviour
{
    [FormerlySerializedAs("velocityMultiplier")]
    public float rangeMultiplier = 1;
    public float fireRateMultiplier = 1;
    public float recoilMultiplier = 1;
    public float energyCostMultiplier = 1;
    public float damageMultiplier = 1;

    bool hasDeployed = false;
        
    public void Deploy(TurretCoreController controller)
    {
        if (hasDeployed)
            return;

        hasDeployed = true;

        DroneBlock block = GetComponent<DroneBlock>();


        float fireRate = block.stats.QueryStat(Stat.FireRateMultiplier);
        if (fireRate != 0)
        {
            fireRateMultiplier = fireRate;
        }
        
        float statRangeMultiplier = block.stats.QueryStat(Stat.RangeMultiplier);
        if (statRangeMultiplier != 0)
        {
            rangeMultiplier = statRangeMultiplier;
        }
        
        float statEnergyCostMultiplier = block.stats.QueryStat(Stat.EnergyCostMultiplier);
        if (statEnergyCostMultiplier != 0)
        {
            energyCostMultiplier = statEnergyCostMultiplier;
        }
        
        controller.rangeMultiplier *= this.rangeMultiplier;
        controller.fireRate *= fireRateMultiplier;
        controller.recoilMultiplier *= recoilMultiplier;
        controller.energyCost *= energyCostMultiplier;
        controller.damageMultiplier *= damageMultiplier;
    }
    
}
