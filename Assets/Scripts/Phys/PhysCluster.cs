using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysCluster : MonoBehaviour
{

    public PhysParent physParent  { get; private set; }

    public Rigidbody rb { get; private set; }
    List<PhysBlock> blocks = new List<PhysBlock>();

    public List<PhysBlock> Blocks() => blocks;

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
        foreach (PhysBlock block in otherCluster.blocks)
        {
            RegisterBlock(block, null );
        }
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
        foreach (PhysBlock block in blocks)
        {
            rb.mass += block.mass;
            block.FinalizeBuild();
        }
        
        CalculateCOM();
    }
    
    
    void CalculateCOM()
    {
        Vector3 com = Vector3.zero;

        float massSum = 0;
        
        foreach (PhysBlock block in blocks)
        {
            com += (block.transform.position + block.transform.rotation * block.centerOfMass) * block.mass;
            massSum += block.mass;
        }

        com /= massSum;
        
        rb.centerOfMass = transform.InverseTransformPoint(com);
        //TODO CALCULATE COM
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rb.worldCenterOfMass, 0.5f);
    }
}
