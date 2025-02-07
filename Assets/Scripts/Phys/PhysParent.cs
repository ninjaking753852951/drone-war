using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
public class PhysParent : NetworkBehaviour
{

    public GameObject physClusterPrefab;
    
    public List<PhysBlock> blocks { get; private set; }

    public NetworkVariable<int> blockCount;

    public List<PhysCluster> clusters { get; set; }

    List<ulong> blockNetIDs;

    public bool networked;
    
    public float totalMass;

    public bool HasBuilt()
    {
        return GetComponentsInChildren<PhysBlock>().Length == blockCount.Value && blockCount.Value != 0;
    }
    
    public void Build()
    {
        GatherAndInitChildrenBlocks();
        
        Clusterize();
        
        SetClusterPositionsToCOM();
        if (networked)
            ShowToNetworkObservers();   
        PhysicsJointAdoption();
        CalculateBlockAdjacency();
        FinalizeBuild();
        DeployDroneController();
    }

    void DeployDroneController()
    {
        GetComponentInChildren<DroneController>().Deploy();
    }

    void GatherAndInitChildrenBlocks()
    {
        blocks = GetComponentsInChildren<PhysBlock>().ToList();
        foreach (PhysBlock block in blocks)
        {
            block.SetPhysParent(this);
        }
        
        if(IsSpawned)
            blockCount.Value = blocks.Count; //ERROR
    }

    void Clusterize()
    {
        clusters = new List<PhysCluster>();
        foreach (PhysBlock block in blocks)
        {
            if (!block.IsInCluster())
            {
                StartNewCluster(block);
            }
        }
    }

    void SetClusterPositionsToCOM()
    {
        foreach (PhysCluster cluster in clusters)
        {
            cluster.CalculateCOM();
        }
    }

    void PhysicsJointAdoption()
    {
        foreach (PhysBlock block in blocks)
        {
            if (block is PhysJointPhysBlock jointBlock)
            {
                jointBlock.InitPhysJoint();
            }
        }
    }

    void ShowToNetworkObservers()
    {
        foreach (ulong clientID in Unity.Netcode.NetworkManager.Singleton.ConnectedClientsIds)
        {
            if(clientID == NetworkManager.Singleton.LocalClientId)
                continue;
            ShowToNetworkObserver(clientID);
        }
    }

    void ShowToNetworkObserver(ulong id)
    {
        GetComponent<NetworkObject>().NetworkShow(id);
        
        foreach (PhysCluster cluster in clusters)
        {
            cluster.GetComponent<NetworkObject>().NetworkShow(id);
            cluster.ShowToNetworkObserver(id);
        }
    }
    
    void StartNewCluster(PhysBlock startPhysBlock)
    {
        PhysCluster newCluster = Instantiate(physClusterPrefab, transform).GetComponent<PhysCluster>();
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkObject netObj = newCluster.GetComponent<NetworkObject>();
            netObj.SpawnWithObservers = false;
            netObj.Spawn();
            /*if(netObj.IsSpawned)
                Debug.Log("PHYS CLUSTER NOT INSTA SPAWNED");*/
            newCluster.transform.parent = transform;
        }

        newCluster.transform.name = "Phys_Cluster_" + clusters.Count;
        newCluster.GetComponent<PhysCluster>().RegisterToPhysParent(this);
        clusters.Add(newCluster);
        startPhysBlock.Init(newCluster, null);
    }
    
    void FinalizeBuild()
    {
        //Debug.Log("FINALIZE BUILD");
        foreach (PhysCluster cluster in clusters)
        {
            cluster.FinalizeBuild();
            totalMass += cluster.rb.mass;
        }
    }

    void CalculateBlockAdjacency()
    {
        foreach (PhysCluster cluster in clusters)
        {
            cluster.CalculateBlockAdjacency();
        }
    }

    public float TotalMass() => totalMass;
}