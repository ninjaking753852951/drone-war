using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityUtils;

public class MachineSaveLoadManager : Singleton<MachineSaveLoadManager>
{

    public string machineDirectory = "Machines";

    public string subAssemblyDirectory = "SubAssemblies";
    
    [System.Serializable]
    public class BlockSaveData
    {
        public Vector3 pos;
        public Vector3 eulerRot;
        public int blockID;

        public BlockSaveData(Vector3 pos, Vector3 eulerRot, int blockID)
        {
            this.pos = pos;
            this.eulerRot = eulerRot;
            this.blockID = blockID;
        }
    }

    [System.Serializable]
    public class MachineSaveData
    {
        public List<BlockSaveData> blocks = new List<BlockSaveData>();
        public float totalCost;
    }
    
    [System.Serializable]
    public class SubAssemblySaveData
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
    }
    

    public void SaveMachine(int slot)
    {
        List<DroneBlock> machineBlocks = FindObjectsByType<DroneBlock>(FindObjectsSortMode.None).ToList();
        MachineSaveData saveData = new MachineSaveData();

        foreach (DroneBlock droneBlock in machineBlocks)
        {
            BlockSaveData blockSaveData = new BlockSaveData(
                droneBlock.transform.position,
                droneBlock.transform.rotation.eulerAngles,
                BlockLibraryManager.Instance.blocks.IndexOf(droneBlock.blockIdentity)
            );
            saveData.blocks.Add(blockSaveData);
        }

        saveData.totalCost = BuildingManager.Instance.totalCost;

        string json = JsonUtility.ToJson(saveData, true);
        string path = GetMachineSlotPath(slot);
        File.WriteAllText(path, json);

        Debug.Log("Machine saved to slot " + slot);
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
            Debug.LogWarning("No save data found in SubAssembly slot ");
            return null;
        }
        
        string json = File.ReadAllText(path);
        SubAssemblySaveData saveData = JsonUtility.FromJson<SubAssemblySaveData>(json);


        return saveData;
    }

    GameObject SpawnSubAssembly(SubAssemblySaveData data, Vector3 pos, Quaternion rot)
    {
        GameObject subAssemblyParent = new GameObject("SubAssemblyParent");
        
        foreach (BlockSaveData blockSaveData in data.blocks)
        {
            if(blockSaveData.blockID >= BlockLibraryManager.Instance.blocks.Count)
                continue;
                
            BlockData blockData = BlockLibraryManager.Instance.blocks[blockSaveData.blockID];
            GameObject newBlock = Instantiate(blockData.prefab, blockSaveData.pos, Quaternion.Euler(blockSaveData.eulerRot));

            newBlock.transform.parent = subAssemblyParent.transform;
                
            DroneBlock droneBlock = newBlock.GetComponent<DroneBlock>();
            droneBlock.blockIdentity = blockData;

        }

        subAssemblyParent.transform.position += pos;
        subAssemblyParent.transform.rotation = rot;

        return subAssemblyParent;
    }
    
    public MachineSaveData LoadMachine(int slot)
    {
        string path = GetMachineSlotPath(slot);

        if (!File.Exists(path))
        {
            Debug.LogWarning("No save data found in slot " + slot);
            return null;
        }
        
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<MachineSaveData>(json);
    }
    
    public DroneController LoadAndSpawnMachine(int slot, bool clearOtherDrones, Vector3 offset = default)
    {
        if (clearOtherDrones)
            Utils.DestroyAllDrones();
        
        MachineSaveData saveData = LoadMachine(slot);

        DroneController droneController = SpawnMachine(saveData, offset);
        
        BuildingManager.Instance.FindDroneController();

        return droneController;
    }

    public DroneController SpawnMachine(MachineSaveData saveData, Vector3 offset = default)
    {
        DroneController droneController = null;
        
        foreach (BlockSaveData blockSaveData in saveData.blocks)
        {
            if(blockSaveData.blockID >= BlockLibraryManager.Instance.blocks.Count)
                continue;
                
            BlockData blockData = BlockLibraryManager.Instance.blocks[blockSaveData.blockID];
            GameObject newBlock = Instantiate(blockData.prefab,blockSaveData.pos + offset,Quaternion.Euler(blockSaveData.eulerRot));
            
            DroneBlock droneBlock = newBlock.GetComponent<DroneBlock>();
            droneBlock.blockIdentity = blockData;

            DroneController curDroneController = newBlock.GetComponent<DroneController>();

            if (curDroneController != null)
            {
                droneController = curDroneController;
            }

        }
        return droneController;
    }

    string GetMachineSlotPath(int x)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, machineDirectory)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, machineDirectory));
        }
        
        return Path.Combine(Application.persistentDataPath, machineDirectory, $"MachineSaveData_Slot{x}.json");
    }
    
    string GetSubAssemblySlotPath(int x)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, subAssemblyDirectory)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, subAssemblyDirectory));
        }
        
        return Path.Combine(Application.persistentDataPath, subAssemblyDirectory, $"SubAssemblySaveData_Slot{x}.json");
    }
    
    void OnGUI()
    {
        if(GameManager.Instance.currentGameMode != GameMode.Build)
            return;
        
        GUILayout.BeginArea(new Rect(10, 10, 200, 300)); // Adjust area size as needed
        GUILayout.Label("Save/Load Machine Slots");

        for (int i = 1; i <= 10; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Slot {i}", GUILayout.Width(50));

            if (GUILayout.Button("Save", GUILayout.Width(60)))
            {
                SaveMachine(i);
            }

            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                MachineSaveData machineData;
                LoadAndSpawnMachine(i, true);
            }

            GUILayout.EndHorizontal();
        }
        
        if (GUILayout.Button("Save Sub-Assembly", GUILayout.Width(120)))
        {
            SaveSubAssembly(0);
        }

        GUILayout.EndArea();
    }
}
