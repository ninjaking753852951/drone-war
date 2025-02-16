using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;
[RequireComponent(typeof(MachineSaveLoadManager))]
public class MachineLibraryManager : Singleton<MachineLibraryManager>
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
            MachineSaveData machine = saveLoad.LoadMachine(i, true);
            if(machine == new MachineSaveData())
                continue;
            loadedMachines.Add(i,machine);
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
