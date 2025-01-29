using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityUtils;
public class MachineInstanceManager : RegulatorSingleton<MachineInstanceManager>
{

    public ulong curID = 0;
    
    Dictionary<ulong, MachineInstance> machines = new Dictionary<ulong, MachineInstance>();

    List<DroneController> drones = new List<DroneController>();
    
    class MachineInstance
    {
        public DroneController controller;
        public GameObject gameObject;

        public MachineInstance(DroneController controller)
        {
            this.controller = controller;
            this.gameObject = controller.gameObject;
        }
    }
    
    public ulong Register(DroneController obj)
    {
        Debug.Log("REGISTERING ");
        curID++;
        machines.Add(curID, new MachineInstance(obj));
        return curID;
    }

    public List<DroneController> FetchAllDrones()
    {
        List<DroneController> drones = new List<DroneController>();
        foreach (var machine in machines)
        {
            if(machine.Value != null)
                drones.Add(machine.Value.controller);
        }

        if (drones.Count == 0)
            Debug.Log("noDronesFound");

        return drones;
    }

    public GameObject FetchGameObject(ulong id)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject;
        }

        if (machines.ContainsKey(id))
        {
            return machines[id].gameObject;
        }
        else
        {
            Debug.LogWarning("Couldnt find drone instance associated with " + id);
            return null;
        }
    }
    
    public ulong FetchID(GameObject obj)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                return netObj.NetworkObjectId;
            }
        }
        
        foreach (var pair in machines)
        {
            if (pair.Value.gameObject == obj)
            {
                return pair.Key; // Return the ID associated with the object
            }
        }

        return 0; // Return 0 if the object is not found
    }

    public void DeregisterDrone(DroneController obj)
    {
        // If the network manager is running, return and do nothing
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            return;
        }

        // If the obj is present in the machine list dictionary, remove it
        foreach (var pair in machines)
        {
            if (pair.Value.controller == obj)
            {
                machines.Remove(pair.Key);
                break;
            }
        }
    }

    
    
}
