using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DroneController : MonoBehaviour, IProgressBar
{
    [Header("Movement Settings")]
    public MovementController movementController;

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

    public Vector3 targetDestination = Vector3.zero;

    [HideInInspector] public Rigidbody rb;
    public float boundingSphereRadius;

    public ulong instanceID;
    bool isNetworkProxy;
    
    void Awake()
    {
        instanceID = MachineInstanceManager.Instance.Register(this);
        rb = GetComponent<Rigidbody>();
        movementController.Initialize(rb, transform, this);
    }

    void Start()
    {

    }

    void Update()
    {
        
        energy.Update(Time.deltaTime);
        
        if (targetDestination == Vector3.zero || movementController == null) return;

        movementController.UpdateMovement(targetDestination, boundingSphereRadius);
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

    public void SetDestination(Vector3 destination)
    {
        targetDestination = destination;
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
        MachineInstanceManager.Instance.DeregisterDrone(this);
    }
}
