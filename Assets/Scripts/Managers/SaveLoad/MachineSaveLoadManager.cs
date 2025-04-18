using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Interfaces;
using Unity.Mathematics;
using Unity.Netcode;
using UnityUtils;

public class MachineSaveLoadManager : Singleton<MachineSaveLoadManager>
{

    public string machineDirectory = "Machines";

    public string subAssemblyDirectory = "SubAssemblies";

    public GameObject physParentPrefab;

    
    [HideInInspector]
    public int curSlot = 0;
    
    [System.Serializable]
    public class BlockSaveData
    {
        public Vector3 pos;
        public Vector3 eulerRot;
        public int blockID;

        public BlockSaveMetaData meta;

        public BlockSaveData(Vector3 pos, Vector3 eulerRot, int blockID, BlockSaveMetaData meta = null)
        {
            this.pos = pos;
            this.eulerRot = eulerRot;
            this.blockID = blockID;
            this.meta = meta;
        }
    }
    
    [System.Serializable]
    public class SubAssemblySaveData : IPlaceable
    {
        public List<BlockSaveData> blocks = new List<BlockSaveData>();
        
        public SubAssemblySaveData()
        {
            DroneController origin = FindObjectOfType<DroneController>();
            List<DroneBlock> machineBlocks = FindObjectsByType<DroneBlock>(FindObjectsSortMode.None).ToList();
            
            foreach (DroneBlock droneBlock in machineBlocks)
            {
                DroneController control = droneBlock.GetComponent<DroneController>();
                if(control != null)
                    continue;
            
                BlockSaveData blockSaveData = new BlockSaveData(
                    droneBlock.transform.position - origin.transform.position - Vector3.up,
                    droneBlock.transform.rotation.eulerAngles,
                    BlockLibraryManager.Instance.blocks.IndexOf(droneBlock.blockIdentity)
                );
                blocks.Add(blockSaveData);
            }
        }

        public string PlaceableName()
        {
            return "Sub Assembly";
        }
        public float Cost()
        {
            return 0;
        }
        public GameObject Spawn(Vector3 pos, Quaternion rot, bool network = true)
        {
            GameObject subAssemblyParent = new GameObject("SubAssemblyParent");
        
            foreach (MachineSaveLoadManager.BlockSaveData blockSaveData in blocks)
            {
                IPlaceable blockData = BlockLibraryManager.Instance.blocks[blockSaveData.blockID];
                if (blockData != null)
                {
                    GameObject blockClone = blockData.Spawn(blockSaveData.pos, Quaternion.Euler(blockSaveData.eulerRot));
                    blockClone.transform.parent = subAssemblyParent.transform;
                }
            }

            subAssemblyParent.transform.position += pos;
            subAssemblyParent.transform.rotation = rot;
            return subAssemblyParent;
        }
        public Sprite Thumbnail()
        {
            Vector3 offset = Vector3.up * 1000 * -1;
            GameObject obj = Spawn(offset, Quaternion.Euler(-35, 35, 0));
            return ThumbnailManager.Instance.GenerateThumbnail(obj);
        }
        public BlockType Category()
        {
            return BlockType.SubAssemblies;
        }
        public BlockStats Stats()
        {
            return null;
        }
    }
    

    public void SaveMachine(int slot)
    {
        List<DroneBlock> machineBlocks = FindObjectsByType<DroneBlock>(FindObjectsSortMode.None).ToList();
        MachineSaveData saveData = new MachineSaveData();
        float totalCost = 0;
        foreach (DroneBlock droneBlock in machineBlocks)
        {

            totalCost += droneBlock.blockIdentity.Cost();
            BlockSaveData blockSaveData = new BlockSaveData(
                droneBlock.transform.position,
                droneBlock.transform.rotation.eulerAngles,
                BlockLibraryManager.Instance.blocks.IndexOf(droneBlock.blockIdentity),
                droneBlock.meta
            );
            saveData.blocks.Add(blockSaveData);
        }

        saveData.totalCost = totalCost;

        string json = saveData.SerializeToJson();
        string path = GetMachineSlotPath(slot);
        File.WriteAllText(path, json);

        DebugLogger.instance.Log("Machine saved to slot " + slot);
    }
    
    public MachineSaveData LoadMachine(int slot, bool dontSetSlot = false)
    {
        if(!dontSetSlot)
            curSlot = slot;
        string path = GetMachineSlotPath(slot);

        if (!File.Exists(path))
        {
            return new MachineSaveData();
        }
        
        string json = File.ReadAllText(path);
        return MachineSaveData.DeserializeFromJson(json);
    }
    
    public MachineSaveData LoadAIMachine(int slot)
    {


        // Get the path for the machine slot
        string path = GetAIMachineSlotPath(slot);

        // Handle AI machines (stored in Resources/AIMachines)
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile == null)
        {
            //Debug.LogWarning($"AI machine at slot {slot} not found. Returning a new MachineSaveData.");
            return new MachineSaveData();
        }

        return MachineSaveData.DeserializeFromJson(jsonFile.text);
    }
    
    public void SaveSubAssembly(int slot)
    {
        SubAssemblySaveData saveData = new SubAssemblySaveData();
        
        string json = JsonUtility.ToJson(saveData, true);
        string path = GetSubAssemblySlotPath(slot);
        File.WriteAllText(path, json);

        Debug.Log("Sub-Assembly saved");
    }

    public List<SubAssemblySaveData> LoadAllSubAssemblies()
    {
        List<SubAssemblySaveData> subAssemblies = new List<SubAssemblySaveData>();
        for (int i = 0; i < 10; i++)
        {
            subAssemblies.Add(LoadSubAssembly(i));
        }

        return subAssemblies;
    }
    
    public SubAssemblySaveData LoadSubAssembly(int slot)
    {
        string path = GetSubAssemblySlotPath(slot);
        

        if (!File.Exists(path))
        {
            return null;
        }
        
        string json = File.ReadAllText(path);
        SubAssemblySaveData saveData = JsonUtility.FromJson<SubAssemblySaveData>(json);


        return saveData;
    }
    
    /*public void LoadAndSpawnMachine(int slot, Vector3 offset = default)
    {
        MachineSaveData saveData = LoadMachine(slot);

        DroneController droneController = null;
        
        if(saveData != null)
            droneController = saveData.Spawn(offset);
        
        if (droneController == null)
        {
            BuildingManager.instance.SpawnDefaultMachine();
        }
        
        BuildingManager.Instance.FindDroneController();
    }*/
    
    string GetMachineSlotPath(int x)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, machineDirectory)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, machineDirectory));
        }
        
        return Path.Combine(Application.persistentDataPath, machineDirectory, $"MachineSaveData_Slot{x}.json");
    }
    
    string GetAIMachineSlotPath(int slotNumber)
    {
        return $"AIMachines/MachineSaveData_Slot{slotNumber}";
    }
    
    string GetSubAssemblySlotPath(int x)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, subAssemblyDirectory)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, subAssemblyDirectory));
        }
        
        return Path.Combine(Application.persistentDataPath, subAssemblyDirectory, $"SubAssemblySaveData_Slot{x}.json");
    }

    public void SwitchMachines(int targetSlot)
    {
        SaveMachine(curSlot);
        curSlot = targetSlot;
        Utils.DestroyAllDrones();
        LoadMachine(curSlot).Spawn(network:false);
        //LoadAndSpawnMachine(curSlot);
    }
}
