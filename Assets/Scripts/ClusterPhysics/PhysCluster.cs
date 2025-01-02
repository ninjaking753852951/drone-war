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

    public void RegisterBlock(PhysBlock physBlock)
    {
        blocks.Add(physBlock);
        physBlock.SetOriginCluster(this);
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
            RegisterBlock(block);
        }
        Destroy(otherCluster.gameObject);
    }

    public void FinalizeBuild()
    {
        foreach (PhysBlock block in blocks)
        {
            rb.mass += block.mass;
            block.FinalizeBuild();
        }
    }
}
