using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.Serialization;

public class TurretCoreController : MonoBehaviour
{
    //public bool isMissile;
    [Header("Missile Settings")]
    public float missileAcceleration = 1;

    [Header("Laser Settings")] 
    public float laserRange = 30;
    public float fireRate = 10;
    public float recoilMultiplier = 1;
    
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

    public TurretType turretType;
    
    public enum TurretType
    {
        Ballistic, Missile, Laser
    }

    public void Deploy(bool deploy)
    {
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

        maxRange = mount.CalculateMaxRange(this);
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
            rangeIndicator.DrawRange(RangeCalculation());

        List<Transform> targets = FindEnemies();
        if (targets == null || targets.Count == 0 || mount == null)
            return;
        target = Utils.ClosestTo(targets, mount.aimPoint.position);
        
        if(turretUpdateTimer.IsFinished)
            AimTurret();


        targetInRange = Vector3.Distance(target.position, mount.aimPoint.position) < RangeCalculation();
        //Debug.Log(fireTimer);
        
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

        TargetFlasher targetFlasher = FindObjectOfType<TargetFlasher>();

        if (targetFlasher != null)
        {
            List<Transform> targetflashers = new List<Transform>();
            targetflashers.Add(targetFlasher.transform);
            return targetflashers;
        }

        
        return validTargets;
    }

    void Fire()
    {
        fireTimer.Reset(1/fireRate);
        fireTimer.Start();
        
        mainBarrel.Fire(this);
    }


    public float drag = 0.3f;         // Unity-style drag coefficient
    public float timeStep = 0.02f;    // Unity's fixed delta time

    public float RangeCalculation()
    {
        return maxRange;
        
        float g = 9.81f; // Gravity (m/s^2)

        // Initial conditions
        Vector2 velocity = new Vector2(shootVelocity * Mathf.Cos(45f * Mathf.Deg2Rad),
            shootVelocity * Mathf.Sin(45f * Mathf.Deg2Rad));
        Vector2 position = Vector2.zero;

        while (position.y >= 0) // Simulate until the projectile hits the ground
        {
            // Apply gravity
            Vector2 acceleration = new Vector2(0, -g);

            // Update velocity with drag applied
            velocity += acceleration * timeStep;
            velocity *= (1 - drag * timeStep);

            // Update position
            position += velocity * timeStep;
        }

        return position.x; // Return horizontal range
    }

    bool ReadyToFire()
    {
        return mount.ReadyToFire() && fireTimer.IsFinished && !mainBarrel.IsObstructed() && targetInRange;
    }
}
