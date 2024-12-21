using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretModule : MonoBehaviour
{
    public float velocityMultiplier = 1;
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
        
        controller.shootVelocityMultiplier *= velocityMultiplier;
        controller.fireRate *= fireRateMultiplier;
        controller.recoilMultiplier *= recoilMultiplier;
        controller.energyCost *= energyCostMultiplier;
        controller.damageMultiplier *= damageMultiplier;
    }
    
}
