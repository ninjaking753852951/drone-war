using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBarrelController : MonoBehaviour
{

    public Transform shootPoint;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    public void Fire(TurretCoreController turret)
    {

        GameObject projectileClone = Instantiate(turret.projectilePrefab, shootPoint.position, shootPoint.rotation);

        Rigidbody rb = projectileClone.GetComponent<Rigidbody>();
        
        rb.AddForce(projectileClone.transform.forward * turret.shootVelocity, ForceMode.VelocityChange);

    }
}
