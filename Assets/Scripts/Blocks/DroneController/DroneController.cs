using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DroneController : MonoBehaviour, IProgressBar
{
    [Header("Movement Settings")]
    public MovementController movementController;
    public DistanceTracker distanceTracker;
    
    [Header("Outline Settings")]
    public Outline outline;
    public float selectionWidth = 2;

    [Header("Misc Settings")]
    public float healthMultiplier = 10;
    public float curHealth;
    public float maxHealth;
    public List<MeshRenderer> coreBlocks;
    public int curTeam;
    List<TurretRangeIndicator> rangeIndicators;

    [Header("Energy Settings")]
    public EnergyController energy;

    public Queue<WaypointManager.Waypoint> waypoints = new Queue<WaypointManager.Waypoint>();


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

        rb = GetComponent<Rigidbody>();
        movementController.Initialize(rb, transform, this);
    }

    void Start()
    {

    }

    void Update()
    {
        
        energy.Update(Time.deltaTime);
        
        if (movementController == null) return;

        movementController.UpdateMovement(TargetDestination());
    }

    public void Select(bool select)
    {
        outline.enabled = select;
        foreach (var rangeIndicator in rangeIndicators)
        {
            rangeIndicator.Select(select);
        }
    }

    public void ClientDeploy()
    {
        isNetworkProxy = true;
        InitOutline();
        InitRangeIndicator();
        energy.Init(this);
        ProgressBarManager.Instance.RegisterHealthBar(this);
        SetCoreColour(MatchManager.Instance.Team(curTeam).colour);
    }

    void InitOutline()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineWidth = selectionWidth;
        outline.enabled = false;
    }

    void InitRangeIndicator()
    {
        rangeIndicators = transform.GetComponentsInChildren<TurretRangeIndicator>().ToList();
    }

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

    public void Deploy()
    {
        instanceID = MachineInstanceManager.Instance.Register(this);
        
        GetComponent<DroneBlock>().Init();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = movementController.mass;
        curHealth *= healthMultiplier;
        maxHealth = curHealth;
        
        energy.Init(this);

        boundingSphereRadius = Utils.CalculateBoundingSphereRadius(rb);
        

        InitOutline();
        InitRangeIndicator();
        
        ProgressBarManager.Instance.RegisterHealthBar(this);

        SetCoreColour(MatchManager.Instance.Team(curTeam).colour);

        movementController.InitializeComponents();
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

    /*public void SetDestination(Vector3 destination)
    {
        TargetDestination() = destination;
    }*/

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
                Utils.DestroyNetworkObjectWithChildren(GetComponent<NetworkObject>());
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
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.worldCenterOfMass, 0.25f);
        
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
    public ProgressBarSettings healthBarSettings;
    public ProgressBarSettings ProgressBarSettings()
    {
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
