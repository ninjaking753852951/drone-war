using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysBlock : MonoBehaviour
{
    [Header("Scan Box Settings")]
    public Vector3 scanBoxSize = new Vector3(1.1f,1.1f,1.1f);
    public Vector3 scanBoxOffset = Vector3.zero;
    public float adjacencyScanBonus = 0;
    
    [Header("Hit box Settings")]
    public Vector3 hitBoxSize = new Vector3(1,1,1);
    public Vector3 hitBoxOffset = Vector3.zero;
    
    [Header("Mass Settings")]
    public float mass = 25;
    public Vector3 centerOfMass;
    
    public PhysCluster originCluster { get; private set; }
    [HideInInspector]
    public UnityEvent onBuildFinalized;

    public List<PhysBlock> neighbors = new List<PhysBlock>();

    PhysParent physParent;
    
    public virtual void Init(PhysCluster originCluster, PhysBlock connectedBlock)
    {
        originCluster.RegisterBlock(this, connectedBlock);
            
        HashSet<PhysBlock> blocks = ScanForNeighboringDroneBlocks();
        HashSet<PhysBlock> blocksToInit = new HashSet<PhysBlock>();

        foreach (PhysBlock physBlock in blocks)
        {
            if (!physBlock.IsInCluster()) // the block has no cluster
            {
                blocksToInit.Add(physBlock);
            }
            else if(physBlock.Cluster() != this.originCluster) // we have encountered a block that is already a part of a cluster
            {
                this.originCluster.MergeCluster(physBlock.Cluster());
            }
        }
        
        foreach (PhysBlock droneBlock in blocksToInit)
        {
            droneBlock.Init(this.originCluster, this);
        }
    }

    public virtual void CalculateAdjacency()
    {
        HashSet<PhysBlock> allNeighbors = ScanForNeighboringDroneBlocks(adjacencyScanBonus);

        foreach (PhysBlock neighbor in allNeighbors)
        {
            if(neighbor != this)
                AddNeighbors(neighbor);
        }
    }
    
    public void FinalizeBuild()
    {
        onBuildFinalized.Invoke();
    }

    public void SetOriginCluster(PhysCluster cluster)
    {
        originCluster = cluster;
        transform.parent = cluster.transform;
    }
    
    public bool IsInCluster()
    {
        return originCluster != null;
    }
    
    public PhysCluster Cluster()
    {
        return originCluster;
    }

    public void SetPhysParent(PhysParent parent)
    {
        physParent = parent;
    }
    
    protected HashSet<PhysBlock> ScanForNeighboringDroneBlocks(float bonus = 0)
    {
        HashSet<PhysBlock> neighboringDroneBlocks = new HashSet<PhysBlock>();
        
        Cuboid scanBox = ScanBox();
        scanBox.size += Vector3.one * bonus;

        foreach (var block in physParent.blocks)
        {
            if(block == this)
                continue;

            if (scanBox.Intersects(block.HitBox()))
                neighboringDroneBlocks.Add(block);
        }

        return neighboringDroneBlocks;
    }  
    
    void OnDrawGizmosSelected()
    {
        if (!IsInCluster())
        {
            //Draw Scanbox
            Cuboid scanBox = ScanBox();
            scanBox.DrawWithGizmos(Color.yellow);
            //Utils.DrawRotatedWireCube(scanBox.position + transform.rotation * scanBoxOffset, scanBox.size, transform.rotation, Color.yellow);   
        
            // Draw Hitbox
            Gizmos.color = Color.green;
            Cuboid hitBox = HitBox();
            hitBox.DrawWithGizmos(Color.green);
            //Utils.DrawRotatedWireCube(hitBox.position + transform.rotation * hitBoxOffset, hitBox.size, transform.rotation, Color.green);   
        }
        
        //Draw Com
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + transform.rotation*centerOfMass, 0.1f);

        Gizmos.color = Color.red;
        foreach (PhysBlock neighbor in neighbors)
        {
            if(neighbor == null)
                continue;
            
            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }

    public void AddNeighbors(PhysBlock neighborblock)
    {
        neighbors.Add(neighborblock);
    }
    
    public void ClearNeighbors()
    {
        neighbors.Clear();
    }

    public Cuboid HitBox()
    {
        return new Cuboid(transform.position + (transform.rotation * hitBoxOffset), hitBoxSize, transform.rotation.eulerAngles);
    }
    
    Cuboid ScanBox()
    {
        return new Cuboid(transform.position + (transform.rotation * scanBoxOffset), scanBoxSize, transform.rotation.eulerAngles);
    }
}

