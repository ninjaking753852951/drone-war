using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBarrelController : MonoBehaviour
{

    public Transform shootPoint;

    public float velocityMultiplier = 1;
    public float fireRateMultiplier = 1;
    public float recoilMultiplier = 1;
    [HideInInspector]
    public float barrelLength;

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Deploy(TurretCoreController controller)
    {
        controller.shootVelocity *= velocityMultiplier;
        controller.fireRate *= fireRateMultiplier;
        controller.recoilMultiplier *= recoilMultiplier;
        barrelLength = Vector3.Distance(controller.transform.position, shootPoint.position);
    }



    /*public void Fire(TurretCoreController turret)
    {

        GameObject projectileClone = Instantiate(turret.projectilePrefab, shootPoint.position, shootPoint.rotation);

        Projectile projectile = projectileClone.GetComponent<Projectile>();
        
        switch (projectile)
        {

            case Bullet bullet:
                bullet.Init(turret);
                //ProjectileInit(turret, projectileClone);
                break;
            case TurretCoreController.TurretType.Missile:
                ProjectileInit(turret, projectileClone);
                break;
            case TurretCoreController.TurretType.Laser:
                LaserInit(turret, projectileClone);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }*/

    void ProjectileInit(TurretCoreController turret ,GameObject projectileClone)
    {
        Rigidbody rb = projectileClone.GetComponent<Rigidbody>();

        float projectileMass = projectileClone.GetComponent<Rigidbody>().mass;
        
        turret.rb.AddForceAtPosition(Vector3.up * projectileMass * turret.shootVelocity * turret.recoilMultiplier, shootPoint.position);
        
        rb.AddForce(projectileClone.transform.forward * turret.shootVelocity, ForceMode.VelocityChange);

        Projectile projectile = projectileClone.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(turret);
        }
    }

    void LaserInit(TurretCoreController turret ,GameObject projectileClone)
    {
        Projectile laserBeam = projectileClone.GetComponent<Projectile>();
        if (laserBeam != null)
        {
            laserBeam.Init(turret);
        }
    }

    public bool IsObstructed()
    {
        bool isObstructed = false;
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position,shootPoint.forward, 2 ))
        {
            isObstructed = true;
            //Debug.Log();
        }

        return false;

        
        return isObstructed;
    }
    
}
