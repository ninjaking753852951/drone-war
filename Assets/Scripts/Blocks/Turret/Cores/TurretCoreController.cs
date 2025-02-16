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

public abstract class TurretCoreController : NetworkBehaviour, IProxyDeploy
{
    public float fireRate = 10;
    public float recoilMultiplier = 1;
    public float energyCost { get; set; }
    public float baseDamage { get; set; }
    public float damageMultiplier = 1;
    [FormerlySerializedAs("shootVelocityMultiplier")]
    public float rangeMultiplier = 1;
    public float shootVelocity;
    
    public Transform target;
    [HideInInspector]
    public TurretBarrelController mainBarrel;
    public List<TurretBarrelController> barrels = new List<TurretBarrelController>();
    public List<TurretMountSingleAxis> mountsSingleAxis = new List<TurretMountSingleAxis>();
    public Rigidbody eldestMountRb;
    CountdownTimer fireTimer;
    bool isDeployed = false;
    [HideInInspector]
    public DroneController controller;
    [HideInInspector]
    public Rigidbody clusterRb;
    public float turretUpdateRate;
    
    [Header("Effects Settings")]
    public VFXData shootVFX;
    
    
    CountdownTimer turretUpdateTimer;
    TurretRangeIndicator rangeIndicator;
    float maxRange;
    NetworkVariable<float> netMaxRange = new NetworkVariable<float>();
    public List<TargetTypes> targetTypes;
    public float aimTolerance = 1; // How off can the angle be in meters but still permit firing
    public ProjectilePoolManager.PooledTypes projectileType;

    [Header("Projectile Simulation")]
    public int simulationSafetyLimit = 500;
    public float simulationStepSize = 0.1f;

    PhysBlock block;    
    float targetPitchAngle;
    float targetYawAngle;
    
    
    protected Quaternion YawRotation()
    {
        Vector3 dir = transform.forward.With(y: 0);
        if (dir != Vector3.zero)
        {
            return Quaternion.LookRotation(transform.forward.With(y: 0));
        }
        else
        {
            return quaternion.identity;
        }
    }
    
    void Awake()
    {
        DroneBlock droneBlock = GetComponent<DroneBlock>();
        baseDamage = droneBlock.stats.QueryStat(Stat.Damage);
        energyCost = droneBlock.stats.QueryStat(Stat.EnergyCost);
        rangeIndicator = GetComponent<TurretRangeIndicator>();
        block = GetComponent<PhysBlock>();
        block.onBuildFinalized.AddListener(Deploy);
    }

    // Start is called before the first frame update
    void Start()
    {
        fireTimer = new CountdownTimer(1 / fireRate);
        fireTimer.Start();
    }
    public void ProxyDeploy()
    {
        Debug.Log("turret proxy deploy");
        //DeployModules();
        //maxRange = MaxRange();
        rangeIndicator.Init(netMaxRange.Value);
    }
    
    
    public void Deploy()
    {
        
        Debug.Log("GUN DEPLOY");
        
        isDeployed = true;
        
        turretUpdateTimer = new CountdownTimer(1 / turretUpdateRate);
        turretUpdateTimer.Start();
        
        controller = transform.root.GetComponentInChildren<DroneController>();

        clusterRb = block.originCluster.rb;
        //clusterRb = Utils.FindParentRigidbody(transform);

        //mountsSingleAxis.Add(FindFirstComponentInAdjacencyMap<TurretMountSingleAxis>(block));
        mountsSingleAxis = FindMountsInAdjacencyMap(block);
        if(mountsSingleAxis.Count > 0)
            eldestMountRb = mountsSingleAxis[^1].block.originCluster.rb;
        
        
        //mountsSingleAxis = GetComponentsInParent<TurretMountSingleAxis>().ToList();
        //Transform eldestMount = Utils.GetHighestInHierarchy(mountsSingleAxis).transform;
        //highestRb = Utils.FindParentRigidbody(eldestMount, eldestMount.GetComponent<Rigidbody>());
        
        
        DeployModules();

        TurretBarrelController barrel = FindFirstComponentInAdjacencyMap<TurretBarrelController>(block);
        if(barrel != null)
            barrels.Add(barrel);
        //barrels = GetComponentsInChildren<TurretBarrelController>().ToList();
        
        if (barrels != null && barrels.Count > 0)
        {
            Debug.Log("MORE THAN ZERO BARRELS");
            mainBarrel = Utils.FurthestFrom(Utils.GetTransformsFromComponents(barrels), transform.position)
                .GetComponent<TurretBarrelController>();   
        }
        else // no barrel so disable the obj
        {
            this.enabled = false;
        }

        maxRange = MaxRange();
        rangeIndicator.Init(maxRange);
        if (IsSpawned)
            netMaxRange.Value = maxRange;
    }
    
    public static T FindFirstComponentInAdjacencyMap<T>(PhysBlock startBlock) where T : Component
    {
        if (startBlock == null)
            return null;
        
        Queue<PhysBlock> queue = new Queue<PhysBlock>();
        HashSet<PhysBlock> visited = new HashSet<PhysBlock>();

        queue.Enqueue(startBlock);
        visited.Add(startBlock);

        while (queue.Count > 0)
        {
            PhysBlock current = queue.Dequeue();
        
            if(current == null)
                continue;

            // Check if this block has the component
            T component = current.GetComponent<T>();
            if (component != null)
            {
                Debug.Log($"Component {typeof(T).Name} found in block: {current.name}");
                return component;
            }

            // Enqueue unvisited neighbors
            foreach (PhysBlock neighbor in current.neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    
        return null;
    }
    
    public static List<TurretMountSingleAxis> FindMountsInAdjacencyMap(PhysBlock startBlock)
    {
        if (startBlock == null)
            return null;
        
        Queue<PhysBlock> queue = new Queue<PhysBlock>();
        HashSet<PhysBlock> visited = new HashSet<PhysBlock>();

        queue.Enqueue(startBlock);
        visited.Add(startBlock);

        List<TurretMountSingleAxis> mounts = new List<TurretMountSingleAxis>();

        bool hasYawMount = false;
        bool hasPitchMount = false;
        
        while (queue.Count > 0)
        {
            PhysBlock current = queue.Dequeue();
        
            if(current == null)
                continue;
            
            // Check if this block has the component
            TurretMountSingleAxis component = current.GetComponent<TurretMountSingleAxis>();
            if (component != null)
            {
                if (!hasYawMount && component.controlType == TurretMountSingleAxis.ControlType.Yaw)
                {
                    mounts.Add(component);
                    hasYawMount = true;
                }
                
                if (!hasPitchMount && component.controlType == TurretMountSingleAxis.ControlType.Pitch)
                {
                    mounts.Add(component);
                    hasPitchMount = true;
                }

                if (hasPitchMount && hasYawMount)
                {
                    //return mounts;   
                }
            }

            // Enqueue unvisited neighbors
            foreach (PhysBlock neighbor in current.neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    
        return mounts;
    }

    
    void DeployModules()
    {
        foreach (PhysBlock neighbor in block.neighbors)
        {
            TurretModule turretModule = neighbor.GetComponent<TurretModule>();
            if (turretModule != null)
            {
                turretModule.Deploy(this);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDeployed || GameManager.Instance.IsOnlineAndClient())
            return;

        AimTurret();
        
        if(target == null)
            return;
        
        if(ReadyToFire())
            Fire();
    }

    void AimTurret()
    {
        turretUpdateTimer.Reset(1/turretUpdateRate);
        turretUpdateTimer.Start();
        
        List<Transform> targets = FindEnemies();
        //Debug.Log("TARGETS  " + targets.Count);
        if (targets == null || targets.Count == 0 )
            return;
        target = Utils.ClosestTo(targets, transform.position);
        
        if(mountsSingleAxis.Count <= 0) 
            return;
        
        //Estimate position accounting for velocity
        Vector3 targetVelocity = Vector3.zero;
        Rigidbody targetRb = target.GetComponentInParent<Rigidbody>();
        if (targetRb != null)
        {
            targetVelocity = targetRb.linearVelocity;
        }
        float interceptTime = EstimateInterceptTime(target.position, targetVelocity);
        Vector3 targetPosEstimate = target.position + targetVelocity * interceptTime;
        
        // Calculate Angles
        targetPitchAngle = -CalculateTargetPitchAngle(targetPosEstimate, interceptTime);
        targetYawAngle = CalculateTargetYawAngle(targetPosEstimate);

        //Debug.Log( "Pitch" + targetPitchAngle + " Yaw " + targetYawAngle);
        
        Vector2 transformedPitchYaw = TransformAngles(targetPitchAngle, targetYawAngle, eldestMountRb.transform.up, eldestMountRb.transform.forward);
        
        //Debug.Log("DEPLOY WITH MOUNTS " + mountsSingleAxis.Count);
        
        foreach (TurretMountSingleAxis turretMountSingleAxis in mountsSingleAxis)
            turretMountSingleAxis.UpdateTurretAngles(transformedPitchYaw.y, transformedPitchYaw.x);
        
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
        //return new List<Transform>();
        int curTeam = controller.curTeam;

        List<DroneController> droneControllers = MachineInstanceManager.Instance.FetchAllDrones();
        List<Transform> validTargets = new List<Transform>();

        foreach (var droneController in droneControllers)
        {
            if(droneController == null)
                continue;
            
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
        
        if(shootVFX != null)
            VFXManager.instance.Spawn(shootVFX, mainBarrel.shootPoint.position, Quaternion.identity);
        
        Shoot();
        
        //mainBarrel.Fire(this);
    }

    protected GameObject SpawnProjectile()
    {
        Vector3 spawnPos = mainBarrel.shootPoint.position;
        
        
        GameObject projectileClone = ProjectilePoolManager.Instance.RequestObject(projectileType, spawnPos).gameObject;

        projectileClone.transform.position = mainBarrel.shootPoint.position;
        projectileClone.transform.rotation = mainBarrel.shootPoint.rotation;  
        
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

        return (targetYawAngle -  eldestMountRb.rotation.eulerAngles.y) %360;
        //return (targetYawAngle ) %360;
        
        /*
        // account for base rotation
        targetYawAngle = (targetYawAngle - aimPoint.rotation.eulerAngles.y) %360;*/
    }

    public abstract float EstimateTimeOfFlight(float initialVelocity, float distance);

    protected virtual bool ReadyToFire()
    {
        DebugLogger.Instance.Log("OBSTRUCTED " +mainBarrel.IsObstructed());
        DebugLogger.Instance.Log("AIMED AT TARGET " +IsAimedAtTarget());
        DebugLogger.Instance.Log("IN RANGE " + TargetInRange() );
        return fireTimer.IsFinished && !mainBarrel.IsObstructed() && TargetInRange() && controller.energy.CanAfford(energyCost) && IsAimedAtTarget();
    }

    protected bool TargetInRange()
    {
        if (target == null)
            return false;
        
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
    
    protected Vector2 TransformAngles(float pitch, float yaw, Vector3 up, Vector3 forward)
    {

        
        Vector3 shootDir = Utils.AnglesToDirection(yaw, pitch);
        //shootDir = highestRb.transform.rotation * shootDir;
        
        Debug.DrawRay(transform.position, shootDir * 10, Color.magenta);
        
        Quaternion offsetRot = Quaternion.FromToRotation(up, Vector3.up);

        shootDir = offsetRot * shootDir;
        
        //shootDir = highestRb.transform.rotation * shootDir;

        Vector2 angles = Utils.DirectionToAngles(shootDir);
        
        return angles;
    }

    public abstract float DamageCalculation();
    
    public float ShootVelocity()
    {
        return shootVelocity * rangeMultiplier;
    }
}
