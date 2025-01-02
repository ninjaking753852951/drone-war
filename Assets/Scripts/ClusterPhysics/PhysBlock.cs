using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysBlock : MonoBehaviour
{
    public Vector3 scanBoxSize = new Vector3(1.1f,1.1f,1.1f);
    public Vector3 scanBoxOffset = Vector3.zero;

    public float mass = 25;
    
    public PhysCluster originCluster { get; private set; }

    public UnityEvent onBuildFinalized;
    
    
    public virtual void Init(PhysCluster originCluster)
    {
        originCluster.RegisterBlock(this);
            
        HashSet<PhysBlock> blocks = ScanForNeighboringDroneBlocks();
        HashSet<PhysBlock> blocksToInit = new HashSet<PhysBlock>();

        foreach (PhysBlock droneBlock in blocks)
        {
            
            if (!droneBlock.IsInCluster()) // the block has no cluster
            {
                blocksToInit.Add(droneBlock);
            }
            else if(droneBlock.Cluster() != this.originCluster) // we have encountered a block that is already a part of a cluster
            {
                this.originCluster.MergeCluster(droneBlock.Cluster());
            }
        }
        
        foreach (PhysBlock droneBlock in blocksToInit)
        {
            droneBlock.Init(this.originCluster);
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
    
    protected HashSet<PhysBlock> ScanForNeighboringDroneBlocks()
    {
        HashSet<PhysBlock> droneBlocks = new HashSet<PhysBlock>();

        // Define the box size for scanning
        Vector3 halfExtents = scanBoxSize / 2f;
    
        // Perform an overlap box check to find all colliders within the box
        Collider[] colliders = Physics.OverlapBox(transform.position + transform.rotation * scanBoxOffset, halfExtents, transform.rotation);

        // Loop through each collider and check if it has a DroneBlock component
        foreach (Collider collider in colliders)
        {
            PhysBlock dronePhysBlock = collider.GetComponent<PhysBlock>();
            if (dronePhysBlock != null && dronePhysBlock != this) // Exclude self
            {
                droneBlocks.Add(dronePhysBlock);
            }  
        }

        return droneBlocks;
    }  
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 halfExtents = scanBoxSize / 2f;
        Gizmos.DrawWireCube(transform.position+ transform.rotation * scanBoxOffset, halfExtents * 2f);
    }
}

