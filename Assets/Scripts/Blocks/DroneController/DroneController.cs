using System;
using System.Collections.Generic;
using System.Linq;
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

    [System.Serializable]
    public class EnergyController : IProgressBar
    {
        [HideInInspector]
        public float energy;
        public float maxEnergy;
        public float energyRegenRate;

        public ProgressBarSettings energyBarSettings;
        
        DroneController controller;

        public void Init(DroneController controller)
        {
            this.controller = controller;
            energy = maxEnergy;
            ProgressBarManager.Instance.RegisterHealthBar(this);
        }

        public void Update(float timeDelta)
        {
            energy = Mathf.MoveTowards(energy, maxEnergy, energyRegenRate * timeDelta);
        }
        public Transform ProgressBarWorldTarget()
        {
            return controller.transform;
        }
        public float ProgressBarFill()
        {
            return energy / maxEnergy;
        }
        public ProgressBarSettings ProgressBarSettings()
        {
            return energyBarSettings;
        }
        public bool IsDestroyed()
        {
            return controller == null;
        }

        public bool DeductEnergy(float amount)
        {
            if (energy - amount >= 0)
            {
                energy -= amount;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanAfford(float amount) => energy - amount >= 0;
    }

    public Vector3 targetDestination = Vector3.zero;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public float boundingSphereRadius;

    public ulong instanceID;

    private void Awake()
    {
        instanceID = MachineInstanceManager.Instance.Register(gameObject);
        rb = GetComponent<Rigidbody>();
        movementController.Initialize(rb, transform, this);
    }

    private void Update()
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

    public void Deploy(bool deploy)
    {
        GetComponent<DroneBlock>().Init();
        rb.isKinematic = !deploy;
        rb.useGravity = deploy;
        rb.mass = movementController.mass;
        curHealth *= healthMultiplier;
        maxHealth = curHealth;
        
        energy.Init(this);

        boundingSphereRadius = Utils.CalculateBoundingSphereRadius(rb);

        if (deploy)
        {
            
            rangeIndicators = transform.GetComponentsInChildren<TurretRangeIndicator>().ToList();


            outline = gameObject.AddComponent<Outline>();
            outline.OutlineWidth = selectionWidth;
            outline.enabled = false;

            ProgressBarManager.Instance.RegisterHealthBar(this);

            Color teamColour = MatchManager.Instance.Team(curTeam).colour;

            foreach (var coreBlock in coreBlocks)
            {
                Renderer rend = coreBlock.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = teamColour;
                }
            }

            movementController.InitializeComponents();
        }
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
            Destroy(gameObject);
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
}
