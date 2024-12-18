using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using Misc;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;
using UnityUtils;

public abstract class TurretCoreController : MonoBehaviour, IProxyDeploy
{
    public float fireRate = 10;
    public float recoilMultiplier = 1;
    public float energyCost;
    public float damageMultiplier = 1;
    
    public Transform target;
    public float shootVelocity;
    [HideInInspector]
    public TurretBarrelController mainBarrel;
    List<TurretBarrelController> barrels;
    //public TurretMountController mount;

    public List<TurretMountSingleAxis> mountSingleAxis;
    [HideInInspector]
    public TurretMountSingleAxis pitchMount;
    [HideInInspector]
    public TurretMountSingleAxis yawMount;
    
    
    CountdownTimer fireTimer;
    bool isDeployed = false;
    
    [HideInInspector]
    public DroneController controller;
    [HideInInspector]
    public Rigidbody rb;

    public float turretUpdateRate;
    CountdownTimer turretUpdateTimer;
    
    TurretRangeIndicator rangeIndicator;

    float maxRange;

    public List<TargetTypes> targetTypes;

    public float aimTolerance = 1; // How off can the angle be in meters but still permit firing
    

    public ObjectPoolManager.PooledTypes projectileType;

    [Header("Projectile Simulation")]
    public int simulationSafetyLimit = 500;
    public float simulationStepSize = 0.1f;

    
    float targetPitchAngle;
    float targetYawAngle;
    
    protected Quaternion YawRotation()
    {
        Vector3 dir = transform.forward.With(y: 0);
        if (dir != Vector3.zero)
        {
            return           Quaternion.LookRotation(transform.forward.With(y: 0));
        }
        else
        {
            return quaternion.identity;
        }
    } 
    
    public void Deploy(bool deploy)
    {
        turretUpdateTimer = new CountdownTimer(1 / turretUpdateRate);
        turretUpdateTimer.Start();
        
        controller = transform.root.GetComponent<DroneController>();

        rb = Utils.FindParentRigidbody(transform, null);
        
        isDeployed = deploy;
        /*mount = GetComponentInParent<TurretMountController>();
        mount.Deploy(this);*/

        mountSingleAxis = GetComponentsInParent<TurretMountSingleAxis>().ToList();
        foreach (TurretMountSingleAxis turretMountSingleAxis in mountSingleAxis)
        {
            turretMountSingleAxis.Deploy(this);
            if (turretMountSingleAxis.controlType == TurretMountSingleAxis.ControlType.Pitch)
                pitchMount = turretMountSingleAxis;
            
            if (turretMountSingleAxis.controlType == TurretMountSingleAxis.ControlType.Yaw)
                yawMount = turretMountSingleAxis;
        }
        
        List<TurretModule> turretModules = GetComponentsInChildren<TurretModule>().ToList();
        foreach (var turretModule in turretModules)
            turretModule.Deploy(this);

        barrels = GetComponentsInChildren<TurretBarrelController>().ToList();
        foreach (var barrel in barrels)
            barrel.Deploy(this);

        if (barrels != null   && barrels.Count > 0)
        {
            mainBarrel = Utils.FurthestFrom(Utils.GetTransformsFromComponents(barrels), transform.position)
                .GetComponent<TurretBarrelController>();   
        }
        else // no barrel so disable the obj
        {
            this.enabled = false;
        }

        maxRange = MaxRange();
        rangeIndicator.SetRange(maxRange);
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
        if(!isDeployed || GameManager.Instance.IsOnlineAndClient())
            return;




        AimTurret();

        
        if(ReadyToFire())
            Fire();
    }

    void AimTurret()
    {
        turretUpdateTimer.Reset(1/turretUpdateRate);
        turretUpdateTimer.Start();
        
        List<Transform> targets = FindEnemies();
        if (targets == null || targets.Count == 0 )
            return;
        target = Utils.ClosestTo(targets, transform.position);
        
        Rigidbody targetRb = target.root.GetComponent<Rigidbody>();

        //Estimate position accounting for velocity
        float interceptTime = EstimateInterceptTime(target.position, targetRb.velocity);
        Vector3 targetPosEstimate = target.position + targetRb.velocity * interceptTime;
        
        // Calculate Angles
        targetPitchAngle = -CalculateTargetPitchAngle(targetPosEstimate, interceptTime);
        targetYawAngle = CalculateTargetYawAngle(targetPosEstimate);
        
        foreach (TurretMountSingleAxis turretMountSingleAxis in mountSingleAxis)
            turretMountSingleAxis.UpdateTurretAngles(targetYawAngle, targetPitchAngle);
        
    }
    
    float EstimateInterceptTime(Vector3 targetPos, Vector3 targetVelocity)
    {
        float projectileVelocity = shootVelocity;
        Vector3 turretPosition = transform.position;

        float time = 0f;
        const float tolerance = 0.01f;
        int maxIterations = simulationSafetyLimit;

        for (int i = 0; i < maxIterations; i++)
        {
            Vector3 predictedTargetPos = targetPos + targetVelocity * time;
            float horizontalDistance = (predictedTargetPos - turretPosition).magnitude;

            float newTime = EstimateTimeOfFlight(projectileVelocity, horizontalDistance);
            if (Mathf.Abs(newTime - time) < tolerance)
                return newTime;

            time = newTime;
        }

        return time;
    }

    List<Transform> FindEnemies()
    {
        int curTeam = controller.curTeam;

        List<DroneController> droneControllers = MachineInstanceManager.Instance.FetchAllDrones();
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
        if (NetworkManager.Singleton.IsListening && !NetworkManager.Singleton.IsServer)
            return;
        
        fireTimer.Reset(1/fireRate);
        fireTimer.Start();

        controller.energy.DeductEnergy(energyCost);
        
        Shoot();
        
        //mainBarrel.Fire(this);
    }

    protected GameObject SpawnProjectile()
    {
        Vector3 spawnPos = mainBarrel.shootPoint.position;
        
        
        GameObject projectileClone = ObjectPoolManager.Instance.RequestObject(projectileType, spawnPos).gameObject;

        /*NetworkTransform netTransform = projectileClone.GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport(mainBarrel.shootPoint.position, mainBarrel.shootPoint.rotation, Vector3.one);
        }*/
        //else
        //{
            projectileClone.transform.position = mainBarrel.shootPoint.position;
            projectileClone.transform.rotation = mainBarrel.shootPoint.rotation;   
        //}
        return projectileClone;
    }
    
    public abstract void Shoot();

    public abstract float MaxRange();

    public abstract float CalculateTargetPitchAngle(Vector3 targetPos, float interceptTime = -1);

    protected float CalculateTargetYawAngle(Vector3 pos)
    {
        Vector3 horizontalDirection = (pos - transform.position).With(y:0);

        // Calculate the yaw angle (angle around the Y-axis)
        float targetYawAngle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;

        return (targetYawAngle -  transform.root.rotation.eulerAngles.y) %360;
        
        /*
        // account for base rotation
        targetYawAngle = (targetYawAngle - aimPoint.rotation.eulerAngles.y) %360;*/
    }

    public abstract float EstimateTimeOfFlight(float initialVelocity, float distance);

    protected virtual bool ReadyToFire()
    {
        
        
        return fireTimer.IsFinished && !mainBarrel.IsObstructed() && TargetInRange() && controller.energy.CanAfford(energyCost) && IsAimedAtTarget();
    }

    protected bool TargetInRange()
    {
        return Vector3.Distance(target.position, transform.position) < maxRange;
    }

    bool IsAimedAtTarget()
    {
        if (target == null)
            return false;

        // Calculate the direction from the turret to the target
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Calculate the yaw and pitch angles for the direction to the target
        float targetYaw = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
        float targetPitch = Mathf.Asin(directionToTarget.y) * Mathf.Rad2Deg;

        // Get the turret's current yaw and pitch angles
        Vector3 turretForward = transform.forward;
        float currentYaw = Mathf.Atan2(turretForward.x, turretForward.z) * Mathf.Rad2Deg;
        float currentPitch = Mathf.Asin(turretForward.y) * Mathf.Rad2Deg;

        // Calculate the angular differences
        float yawDifference = Mathf.DeltaAngle(currentYaw, targetYaw);
        float pitchDifference = Mathf.DeltaAngle(currentPitch, targetPitch);

        // Check if both yaw and pitch differences are within the aim tolerance
        return Mathf.Abs(yawDifference) <= aimTolerance && Mathf.Abs(pitchDifference) <= aimTolerance;
    }



    public void ProxyDeploy()
    {
        maxRange = MaxRange();
        rangeIndicator.SetRange(maxRange);
    }

    public abstract float DamageCalculation();
}
