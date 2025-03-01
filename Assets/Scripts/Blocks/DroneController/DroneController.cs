using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DroneController : NetworkBehaviour, IProgressBar
{
    [Header("Movement Settings")]
    public MovementController movementController;
    public DistanceTracker distanceTracker;
    [HideInInspector]
    public NetworkVariable<bool> isAccelerating;
    
    [Header("Outline Settings")]
    public Outline outline;
    public float selectionWidth = 2;
    
    [Header("Healthbar Settings")]
    public ProgressBarSettings healthBarSettings;
    public Color localPlayerHealthbarColour;
    
    [Header("Misc Settings")]
    public float healthMultiplier = 10;
    public float curHealth;
    public float maxHealth;

    public List<MeshRenderer> coreBlocks;
    public int curTeam;
    List<TurretRangeIndicator> rangeIndicators = new List<TurretRangeIndicator>();

    [Header("Energy Settings")]
    public EnergyController energy;
    
    public Queue<WaypointManager.Waypoint> waypoints = new Queue<WaypointManager.Waypoint>();

    [HideInInspector]
    public PhysBlock physBlock;

    DroneLogicController logicController;
    
    public Vector3 TargetDestination()
    {
        if (waypoints != null && waypoints.Count != 0)
        {
            WaypointManager.Waypoint waypoint = waypoints.Peek();
            return waypoint.waypointMarker.transform.position;
        }
        
        return Vector3.zero;
    } 

    [HideInInspector] public Rigidbody rb;
    public float boundingSphereRadius;

    [HideInInspector]
    public UnityEvent<DroneController> onDroneDestroyed = new UnityEvent<DroneController>();
    
    public ulong instanceID;
    bool isNetworkProxy;
    
    void Awake()
    {
        physBlock = GetComponent<PhysBlock>();
        //physBlock.onBuildFinalized.AddListener(Deploy);
    }

    void Start()
    {

    }

    void Update()
    {
        if(isNetworkProxy)
            ProxyUpdate();
        
        if(!physBlock.IsInCluster())
            return;
        
        
        energy.Update(Time.deltaTime);
        
        if (movementController == null) return;

        movementController.UpdateMovement(TargetDestination());
    }

    public void Select(bool select)
    {
        outline.enabled = select;
        //return;
        foreach (var rangeIndicator in rangeIndicators)
        {
            rangeIndicator.SetVisible(select);
        }
    }

    void InitOutline()
    {
        List<TurretRangeIndicator> turretRangeIndicators = transform.root.GetComponentsInChildren<TurretRangeIndicator>().ToList();
        foreach (TurretRangeIndicator indicator in turretRangeIndicators)
            indicator.SetVisible(false);
        
        outline = transform.root.gameObject.AddComponent<Outline>();
        outline.OutlineWidth = selectionWidth;
        outline.enabled = false;
        
        foreach (TurretRangeIndicator indicator in turretRangeIndicators)
            indicator.SetVisible(false);

        rangeIndicators = turretRangeIndicators;
    }

    /*void InitRangeIndicator()
    {
        rangeIndicators = transform.root.GetComponentsInChildren<TurretRangeIndicator>().ToList();
        foreach (TurretRangeIndicator rangeIndicator in rangeIndicators)
        {
            rangeIndicator.Init();
        }
    }*/

    void SetCoreColour(Color teamColour)
    {
        foreach (var coreBlock in coreBlocks)
        {
            Renderer rend = coreBlock.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = teamColour;
            }
        }
    }

    public void ProxyUpdate()
    {
        movementController.sound.throttle = isAccelerating.Value;
    }
    
    public void ProxyDeploy()
    {
        movementController.sound.mute = false;
        isNetworkProxy = true;
        InitOutline();
        //InitRangeIndicator();
        energy.Init(this);
        InitHealth();
        //ProgressBarManager.Instance.RegisterHealthBar(this);
        SetCoreColour(MatchManager.Instance.Team(curTeam).colour);
    }
    
    public void Deploy()
    {
        instanceID = MachineInstanceManager.Instance.Register(this);

        rb = physBlock.Cluster().rb;
        
        InitMovement();
        
        InitOutline();
        
        InitHealth();
        
        energy.Init(this);
        
        //InitRangeIndicator();
        
        SetCoreColour(MatchManager.Instance.Team(curTeam).colour);

        logicController = GetComponent<DroneLogicController>();
        logicController.Init();
    }

    void InitHealth()
    {
        List<DroneBlock> droneBlocks = transform.root.GetComponentsInChildren<DroneBlock>().ToList();
        curHealth = 0;
        foreach (DroneBlock droneBlock in droneBlocks)
        {
            curHealth += droneBlock.stats.QueryStat(Stat.HitPoints);
        }

        curHealth *= healthMultiplier;
        maxHealth = curHealth;
        WorldUIManager.Instance.healthBarManager.RegisterHealthBar(this);
    }

    void InitMovement()
    {
        movementController.Initialize(rb, transform, this);
        movementController.mass = physBlock.Cluster().physParent.TotalMass();
        movementController.InitializeComponents();
        boundingSphereRadius = Utils.CalculateBoundingSphereRadius(physBlock.Cluster().physParent.transform);
    }
    
    public void ReachedWaypoint()
    {
        if(waypoints.Count == 0)
            return;
        
        WaypointManager.Instance.DisposeWaypoint(waypoints.Dequeue());
    }

    public void ClearWaypoints()
    {
        foreach (var waypoint in waypoints)
        {
            waypoint.Dispose();
        }
        waypoints.Clear();
    }

    public void AddWaypoint(WaypointManager.Waypoint waypoint)
    {
        waypoints.Enqueue(waypoint);
    }
    
    public void TakeDamage(float damage)
    {
        curHealth -= damage;

        if (curHealth <= 0)
        {
            if (NetworkManager.Singleton.IsListening)
            {
                Utils.DestroyNetworkObjectWithChildren(transform.root.GetComponent<NetworkObject>());
            }
            else
            {
                Destroy(gameObject);   
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if(rb == null)
            return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.worldCenterOfMass, boundingSphereRadius);
    }
    public Transform ProgressBarWorldTarget()
    {
        return transform;
    }
    public float ProgressBarFill()
    {
        return curHealth / maxHealth;
    }
    public float ProgressBarMaximum()
    {
        return maxHealth;
    }
    public ProgressBarSettings ProgressBarSettings()
    {
        //TODO find out if we are a local team machine

        if (MatchManager.Instance.IsPlayerOwned(this))
            healthBarSettings.colour = localPlayerHealthbarColour;
        
        return healthBarSettings;
    }
    public bool IsDestroyed()
    {
        return this == null;
    }
    
    void OnDestroy()
    {
        onDroneDestroyed.Invoke(this);
        MachineInstanceManager.Instance.DeregisterDrone(this);
    }
}
