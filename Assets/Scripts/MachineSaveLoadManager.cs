using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class MachineSaveLoadManager : MonoBehaviour
{
    [System.Serializable]
    class BlockSaveData
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
    class MachineSaveData
    {
        public List<BlockSaveData> blocks = new List<BlockSaveData>();
    }

    void Update()
    {
        // remove this in favor of the gui 
    }

    void SaveMachine(int slot)
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

        string json = JsonUtility.ToJson(saveData, true);
        string path = GetSlotFilePath(slot);
        File.WriteAllText(path, json);

        Debug.Log("Machine saved to slot " + slot);
    }

    void LoadMachine(int slot)
    {
        List<DroneBlock> machineBlocks = FindObjectsByType<DroneBlock>(FindObjectsSortMode.None).ToList();
        foreach (DroneBlock machineBlock in machineBlocks)
        {
            Destroy(machineBlock.gameObject);
        }

        string path = GetSlotFilePath(slot);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            MachineSaveData saveData = JsonUtility.FromJson<MachineSaveData>(json);

            foreach (BlockSaveData blockSaveData in saveData.blocks)
            {
                BlockData blockData = BlockLibraryManager.Instance.blocks[blockSaveData.blockID];
                GameObject newBlock = Instantiate(blockData.prefab);
                newBlock.transform.position = blockSaveData.pos;
                newBlock.transform.eulerAngles = blockSaveData.eulerRot;

                DroneBlock droneBlock = newBlock.GetComponent<DroneBlock>();
                droneBlock.blockIdentity = blockData;
            }

            Debug.Log("Machine loaded from slot " + slot);
        }
        else
        {
            Debug.LogWarning("No save data found in slot " + slot);
        }

        BuildingManager.Instance.FindDroneController();
    }

    string GetSlotFilePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"MachineSaveData_Slot{slot}.json");
    }
    
    void OnGUI()
    {
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
                LoadMachine(i);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }
}
