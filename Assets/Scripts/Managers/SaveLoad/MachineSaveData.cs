using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class MachineSaveData
{
    public List<MachineSaveLoadManager.BlockSaveData> blocks = new List<MachineSaveLoadManager.BlockSaveData>();
    public float totalCost;

    public DroneController Spawn(Vector3 offset = default, Vector3 eulerRot = default, Transform parentParent = null, int teamID = 0, bool network = true, bool deploy = false)
    {
        Transform parent = GetParentTransform(parentParent, deploy, network, out GameObject physParent, out NetworkObject physParentNetObj);
        DroneController droneController = null;
        List<ulong> blockNetIDs = new List<ulong>();

        foreach (MachineSaveLoadManager.BlockSaveData blockSaveData in blocks)
        {
            if (!TrySpawnBlock(blockSaveData, offset, eulerRot, network, parent, out GameObject newBlock, out DroneController curDroneController))
                continue;

            /*if (network)
            {
                HandleNetworkBlock(newBlock, blockNetIDs);
            }
            else
            {
                newBlock.transform.parent = parent;
            }*/
            
            newBlock.transform.parent = parent;

            InjectBlockMetadata(newBlock, blockSaveData);
            droneController = UpdateDroneController(curDroneController, droneController, teamID, blocks.Count);
        }

        if (droneController == null)
        {
            droneController = BlockLibraryManager.Instance.coreBlock.Spawn(BuildingManager.Instance.spawnPoint, Quaternion.Euler(eulerRot), false).GetComponent<DroneController>();
            droneController.transform.parent = parent;
        }

        if (deploy)
        {
            HandleDeployment(droneController, physParent, blockNetIDs, network);
        }

        return droneController;
    }

    private Transform GetParentTransform(Transform parentParent, bool deploy, bool network, out GameObject physParent, out NetworkObject physParentNetObj)
    {
        physParent = null;
        physParentNetObj = null;

        if (deploy)
        {
            physParent = GameObject.Instantiate(MachineSaveLoadManager.Instance.physParentPrefab, parentParent);
            if (network)
            {
                physParentNetObj = physParent.GetComponent<NetworkObject>();
                physParentNetObj.SpawnWithObservers = false;
                physParentNetObj.Spawn();
            }
            return physParent.transform;
        }

        return parentParent;
    }

    private bool TrySpawnBlock(MachineSaveLoadManager.BlockSaveData blockSaveData, Vector3 offset, Vector3 eulerRot, bool network, Transform parent, out GameObject newBlock, out DroneController curDroneController)
    {
        newBlock = null;
        curDroneController = null;

        IPlaceable blockData = BlockLibraryManager.Instance.BlockData(blockSaveData.blockID);
        if (blockData == null)
            return false;

        Quaternion rotation = Quaternion.Euler(eulerRot) * Quaternion.Euler(blockSaveData.eulerRot);
        Vector3 position = Quaternion.Euler(eulerRot) * blockSaveData.pos + offset;

        newBlock = blockData.Spawn(position, rotation, network);
        curDroneController = newBlock.GetComponent<DroneController>();

        return true;
    }

    private void HandleNetworkBlock(GameObject newBlock, List<ulong> blockNetIDs)
    {
        NetworkObject blockNetObj = newBlock.GetComponent<NetworkObject>();
        if (blockNetObj != null)
        {
            blockNetIDs.Add(blockNetObj.NetworkObjectId);
        }
        else
        {
            Debug.Log("BLOCK DOESNT HAVE NET OBJ " + newBlock.name);
        }
    }

    private void InjectBlockMetadata(GameObject newBlock, MachineSaveLoadManager.BlockSaveData blockSaveData)
    {
        DroneBlock droneBlock = newBlock.GetComponent<DroneBlock>();
        if (blockSaveData.meta != null && droneBlock != null)
        {
            droneBlock.meta = blockSaveData.meta;
        }
    }

    private DroneController UpdateDroneController(DroneController curDroneController, DroneController droneController, int teamID, int blockCount)
    {
        if (curDroneController != null)
        {
            droneController = curDroneController;
            droneController.curTeam = teamID;

            if (NetworkManager.Singleton.IsListening)
            {
                DroneNetworkController networkController = curDroneController.GetComponent<DroneNetworkController>();
                if (networkController != null)
                {
                    networkController.blockCount.Value = blockCount;
                    networkController.curTeam.Value = teamID;
                }
            }
        }

        return droneController;
    }

    private void HandleDeployment(DroneController droneController, GameObject physParent, List<ulong> blockNetIDs, bool network)
    {
        if (network)
        {
            PhysParent physParentController = physParent.GetComponent<PhysParent>();
            physParentController.networked = true;
            //physParentController.SetBlockList(blockNetIDs.ToArray());
            physParentController.Build();
        }
        else
        {
            PhysParent physParentController = physParent.GetComponent<PhysParent>();
            physParentController.networked = false;
            physParentController.Build();
        }
    }

    public Sprite GenerateThumbnail()
    {
        Vector3 posOffset = Vector3.down * 1000; // Ensure it spawns out of view
        GameObject machineParent = new GameObject();
        DroneController droneController = Spawn(posOffset, new Vector3(-35, 35, 0), machineParent.transform, network: false, deploy: false);
        if (droneController == null)
        {
            return null;
        }

        Sprite thumbnail = ThumbnailManager.Instance.GenerateThumbnail(machineParent, 0.1f);
        return thumbnail;
    }

    public string SerializeToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public static MachineSaveData DeserializeFromJson(string json)
    {
        return JsonUtility.FromJson<MachineSaveData>(json);
    }
}