using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;
[RequireComponent(typeof(MachineSaveLoadManager))]
public class MachineLibrary : Singleton<MachineLibrary>
{
    public Dictionary<int,MachineSaveData> loadedMachines = new Dictionary<int,MachineSaveData>();

    public float debugLoadedMachinesCount;
    
    MachineSaveLoadManager saveLoad;
    
    protected override void Awake()
    {
        base.Awake();
        saveLoad = GetComponent<MachineSaveLoadManager>();
        LoadLocalMachines();
    }

    void LoadLocalMachines()
    {
        for (int i = 0; i < 10; i++)
        {
            loadedMachines.Add(i,saveLoad.LoadMachine(i, true));
        }
    }

    void Update()
    {
        debugLoadedMachinesCount = loadedMachines.Count;
    }


    public MachineSaveData FetchMachine(int index)
    {
        if (loadedMachines.ContainsKey(index))
        {
            return loadedMachines[index];  
        }
        else
        {
            Debug.Log("No machine loaded at index " + index);
            return null;
        }
    }
}
