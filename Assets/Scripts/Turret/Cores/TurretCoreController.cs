using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using Misc;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class TurretCoreController : MonoBehaviour
{
    public float fireRate = 10;
    public float recoilMultiplier = 1;
    public float energyCost;
    
    public GameObject projectilePrefab;
    public Transform target;
    public float shootVelocity;
    [HideInInspector]
    public TurretBarrelController mainBarrel;
    List<TurretBarrelController> barrels;
    public TurretMountController mount;

    CountdownTimer fireTimer;
    bool isDeployed = false;
    bool targetInRange;
    
    [HideInInspector]
    public DroneController controller;
    [HideInInspector]
    public Rigidbody rb;

    public float turretUpdateRate;
    CountdownTimer turretUpdateTimer;
    
    TurretRangeIndicator rangeIndicator;

    float maxRange;

    public List<TargetTypes> targetTypes;
    

    public float shootHeightOffset = 0f;

    public void Deploy(bool deploy)
    {
        Debug.Log("Deploying");
        turretUpdateTimer = new CountdownTimer(1 / turretUpdateRate);
        turretUpdateTimer.Start();
        
        controller = transform.root.GetComponent<DroneController>();

        rb = Utils.FindParentRigidbody(transform, null);
        
        isDeployed = deploy;
        mount = GetComponentInParent<TurretMountController>();
        mount.Deploy(this);
        
        List<TurretModule> turretModules = GetComponentsInChildren<TurretModule>().ToList();
        foreach (var turretModule in turretModules)
            turretModule.Deploy(this);

        barrels = GetComponentsInChildren<TurretBarrelController>().ToList();
        foreach (var barrel in barrels)
            barrel.Deploy(this);
        
        mainBarrel = Utils.FurthestFrom(Utils.GetTransformsFromComponents(barrels), transform.position)
            .GetComponent<TurretBarrelController>();

        maxRange = MaxRange();
    }

    void Awake()
    {
        rangeIndicator = GetComponent<TurretRangeIndicator>();
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
        
        if(rangeIndicator != null)
            rangeIndicator.DrawRange(maxRange);

        List<Transform> targets = FindEnemies();
        if (targets == null || targets.Count == 0 || mount == null)
            return;
        target = Utils.ClosestTo(targets, mount.aimPoint.position);
        
        if(turretUpdateTimer.IsFinished)
            AimTurret();


        targetInRange = Vector3.Distance(target.position, mount.aimPoint.position) < maxRange;

        
        if(ReadyToFire())
            Fire();
    }

    void AimTurret()
    {
        turretUpdateTimer.Reset(1/turretUpdateRate);
        turretUpdateTimer.Start();
        
        Rigidbody targetRb = target.root.GetComponent<Rigidbody>();
        mount.UpdateTurretAim(this, target.position,targetRb.velocity);
    }

    List<Transform> FindEnemies()
    {
        int curTeam = controller.curTeam;

        List<DroneController> droneControllers = FindObjectsOfType<DroneController>().ToList();
        List<Transform> validTargets = new List<Transform>();

        foreach (var droneController in droneControllers)
        {
            if (droneController.transform.root != transform.root)
            {
                if (droneController.curTeam != curTeam)
                {
                    validTargets.Add(droneController.transform);
                }
            }
        }

        List<IDamageable> damageables = DamageableManager.Instance.FetchDamageables();

        foreach (IDamageable damageable in damageables)
        {
            if (damageable.Team() != curTeam)
            {
                if(targetTypes.Contains(damageable.TargetType()))
                    validTargets.Add(damageable.Transform());
            }
        }
        
        return validTargets;
    }

    void Fire()
    {
        fireTimer.Reset(1/fireRate);
        fireTimer.Start();

        controller.energy.DeductEnergy(energyCost);
        
        Shoot();
        
        //mainBarrel.Fire(this);
    }

    public abstract void Shoot();

    public abstract float MaxRange();

    public abstract float CalculateTargetPitchAngle(Vector3 targetPos, float interceptTime = -1);

    public abstract float EstimateTimeOfFlight(float initialVelocity, float distance);

    protected virtual bool ReadyToFire()
    {
        return mount.ReadyToFire() && fireTimer.IsFinished && !mainBarrel.IsObstructed() && targetInRange && controller.energy.CanAfford(energyCost);
    }


}
