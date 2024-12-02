using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretModule : MonoBehaviour
{
    public float velocityMultiplier = 1;
    public float fireRateMultiplier = 1;
    public float recoilMultiplier = 1;

    public void Deploy(TurretCoreController controller)
    {
        controller.shootVelocity *= velocityMultiplier;
        controller.fireRate *= fireRateMultiplier;
        controller.recoilMultiplier *= recoilMultiplier;
    }
    
}
