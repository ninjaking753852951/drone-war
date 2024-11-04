using System.Collections;
using System.Collections.Generic;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.Serialization;

public class TurretCoreController : MonoBehaviour
{

    public float fireRate = 10;
    
    float timer;
    

    public GameObject projectilePrefab;

    public Transform target;

    public float shootVelocity;

    public TurretBarrelController barrel;
    public TurretMountController mount;

    CountdownTimer fireTimer;

    bool isDeployed = false;

    public void Deploy(bool deploy)
    {
        isDeployed = deploy;
        mount.Deploy(deploy);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        fireTimer = new CountdownTimer(1 / fireRate);
        fireTimer.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDeployed)
            return;

        List<Transform> targets = TargetManager.Instance.GetTargets();
        if (targets == null)
            return;
        target = Utils.ClosestTo(targets, mount.aimPoint.position);
        
        mount.UpdateTurretAim(this, target.position);
        
        Debug.Log(fireTimer);
        
        if(mount.ReadyToFire() && fireTimer.IsFinished)
            Fire();
    }

    void Fire()
    {
        fireTimer.Reset(1/fireRate);
        fireTimer.Start();
        
        barrel.Fire(this);
    }
}
