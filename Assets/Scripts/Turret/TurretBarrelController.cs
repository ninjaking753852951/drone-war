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



    public void Fire(TurretCoreController turret)
    {

        GameObject projectileClone = Instantiate(turret.projectilePrefab, shootPoint.position, Quaternion.LookRotation(shootPoint.up));

        Rigidbody rb = projectileClone.GetComponent<Rigidbody>();

        float projectileMass = projectileClone.GetComponent<Rigidbody>().mass;
        
        turret.rb.AddForceAtPosition(Vector3.up * projectileMass * turret.shootVelocity * turret.recoilMultiplier, shootPoint.position);
        
        rb.AddForce(projectileClone.transform.forward * turret.shootVelocity, ForceMode.VelocityChange);
        
        
        Projectile projectile = projectileClone.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(turret.controller,  turret.drag, turret.missileAcceleration);
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
