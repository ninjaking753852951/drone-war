using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysCluster : MonoBehaviour
{

    public PhysParent physParent  { get; private set; }

    public Rigidbody rb { get; private set; }
    public HashSet<PhysBlock> blocks = new HashSet<PhysBlock>();

    public List<PhysBlock> Blocks() => blocks.ToList();

    public Vector3 com { get; set; }
    
    Dictionary<PhysBlock, PhysBlock> adjacencyMap = new Dictionary<PhysBlock, PhysBlock>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ShowToNetworkObserver(ulong id)
    {
        foreach (PhysBlock block in blocks)
        {
            NetworkObject netObj = block.GetComponent<NetworkObject>();
            if (!netObj.IsNetworkVisibleTo(id))
            {
                netObj.NetworkShow(id);   
            }
        }
    }

    public void EnablePhysics()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    public void RegisterToPhysParent(PhysParent parent)
    {
        physParent = parent;
    }

    public void RegisterBlock(PhysBlock physBlock, PhysBlock neighborBlock)
    {
        blocks.Add(physBlock);
        physBlock.SetOriginCluster(this);
        //physBlock.AddNeighbors(neighborBlock);
    }

    public void MergeCluster(PhysCluster otherCluster)
    {
        if (Blocks().Count > otherCluster.Blocks().Count)
        {
            AssimilateCluster(otherCluster);
        }
        else
        {
            otherCluster.AssimilateCluster(this);
        }
    }

    public void AssimilateCluster(PhysCluster otherCluster)
    {
        Debug.Log("ASSImilation");
        foreach (PhysBlock block in otherCluster.blocks)
        {
            RegisterBlock(block, null );
        }
        
        /*// Incase this was spawned
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn(false);*/

        physParent.clusters.Remove(otherCluster);
        Destroy(otherCluster.gameObject);
    }

    public void CalculateBlockAdjacency()
    {
        foreach (PhysBlock block in blocks)
        {
            block.CalculateAdjacency();
        }
    }

    public void FinalizeBuild()
    {
        //Debug.Log( blocks.Count);
        foreach (PhysBlock block in blocks)
        {
            rb.mass += block.Mass();
            block.FinalizeBuild();
        }
        
        //Debug.Log(massSum);
        
        EnablePhysics();

        //CalculateCOM();
    }
    
    
    public void CalculateCOM()
    {
        
        
        com = Vector3.zero;

        float massSum = 0;
        
        foreach (PhysBlock block in blocks)
        {
            float blockMass = block.Mass();
            com += (block.transform.position + block.transform.rotation * block.centerOfMass) * blockMass;
            massSum += blockMass;
        }

        com /= massSum;
        
        Utils.MoveParentWithoutAffectingChildren(transform, com);
        com = transform.InverseTransformPoint(com);
        rb.centerOfMass = com;
        //Debug.Log(rb.transform.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rb.worldCenterOfMass, 0.5f);
    }
}
