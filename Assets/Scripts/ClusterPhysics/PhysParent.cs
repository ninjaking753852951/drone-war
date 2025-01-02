using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PhysParent : MonoBehaviour
{

    public GameObject physClusterPrefab;

    List<PhysCluster> clusters = new List<PhysCluster>();

    public bool deployOnStart;

    float totalMass;
    
    void Start()
    {
        if (deployOnStart)
        {
            Build(); 
        }
    }

    public void Build()
    {
        Clusterize();
        PhysicsJointAdoption();
        FinalizeBuild();
    }

    void Clusterize()
    {
        List<PhysBlock> blocks = FindObjectsByType<PhysBlock>(FindObjectsSortMode.None).ToList();

        foreach (PhysBlock block in blocks)
        {
            if (!block.IsInCluster())
            {
                StartNewCluster(block);
            }
        }
    }

    void PhysicsJointAdoption()
    {
        List<PhysJointPhysBlock> blocks = FindObjectsByType<PhysJointPhysBlock>(FindObjectsSortMode.None).ToList();

        foreach (PhysJointPhysBlock block in blocks)
        {
            block.InitPhysJoint();
        }
    }
    
    void StartNewCluster(PhysBlock startPhysBlock)
    {
        PhysCluster newCluster = Instantiate(physClusterPrefab, transform).GetComponent<PhysCluster>();
        newCluster.transform.name = "Phys_Cluster_" + clusters.Count;
        newCluster.GetComponent<PhysCluster>().RegisterToPhysParent(this);
        clusters.Add(newCluster);
        startPhysBlock.Init(newCluster);
    }

    void FinalizeBuild()
    {
        foreach (PhysCluster cluster in clusters)
        {
            cluster.FinalizeBuild();
            totalMass += cluster.rb.mass;
        }
    }

    public float TotalMass() => totalMass;
}