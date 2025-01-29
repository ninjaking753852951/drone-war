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
    
    public NetworkVariable<int> childCount;

    List<PhysCluster> clusters = new List<PhysCluster>();

    List<ulong> blockNetIDs;

    public bool networked;
    
    float totalMass;

    bool hasBuilt = false;


    void Update()
    {
        if (!hasBuilt && blockNetIDs != null && NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < blockNetIDs.Count; i++)
            {
                ulong blockNetID = blockNetIDs[i];
                if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(blockNetID))
                    return;

            }

            Build();
            hasBuilt = true;
        }
    }

    public void SetBlockList(ulong[] blockNetIDs)
    {
        //Debug.Log("Recieved block list");
        this.blockNetIDs = blockNetIDs.ToList();
    }
    
    
    public void Build()
    {
        /*if(!Unity.Netcode.NetworkManager.Singleton.IsServer)
            return;*/

        /*if (networked)
        {
            NetGatherAndInitChildrenBlocks();
        }
        else
        {
            GatherAndInitChildrenBlocks();
        }*/
        
        GatherAndInitChildrenBlocks();
        
        Clusterize();
        
        childCount.Value = transform.childCount;
        
        SetClusterPositionsToCOM();
        if (networked)
        {
            ShowToNetworkObservers();   
        }
        PhysicsJointAdoption();
        CalculateBlockAdjacency();
        FinalizeBuild();
        DeployDroneController();
    }

    void DeployDroneController()
    {
        GetComponentInChildren<DroneController>().Deploy();
    }
    
    void NetGatherAndInitChildrenBlocks()
    {
        blocks = new List<PhysBlock>();
        
        for (int i = 0; i < blockNetIDs.Count; i++)
        {
            ulong blockNetID = blockNetIDs[i];
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(blockNetID))
            {
                blocks.Add(NetworkManager.Singleton.SpawnManager.SpawnedObjects[blockNetID].GetComponent<PhysBlock>());
            }
        }
        
        foreach (PhysBlock block in blocks)
        {
            block.SetPhysParent(this);
        }
    }

    void GatherAndInitChildrenBlocks()
    {
        blocks = GetComponentsInChildren<PhysBlock>().ToList();
        foreach (PhysBlock block in blocks)
        {
            block.SetPhysParent(this);
        }
    }

    void Clusterize()
    {
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
            newCluster.transform.parent = transform;
        }

        newCluster.transform.name = "Phys_Cluster_" + clusters.Count;
        newCluster.GetComponent<PhysCluster>().RegisterToPhysParent(this);
        clusters.Add(newCluster);
        startPhysBlock.Init(newCluster, null);
    }
    
    void FinalizeBuild()
    {
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