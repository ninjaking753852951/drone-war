using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityUtils;
public class MachineInstanceManager : Singleton<MachineInstanceManager>
{

    ulong curID = 0;
    
    Dictionary<ulong, GameObject> machines = new Dictionary<ulong, GameObject>();

    public ulong Register(GameObject obj)
    {
        curID++;
        machines.Add(curID, obj);
        return curID;
    }

    public GameObject FetchGameObject(ulong id)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            return NetworkManager.Singleton.SpawnManager.SpawnedObjects[id].gameObject;
        }
        
        if (machines.TryGetValue(id, out GameObject fetch))
            return fetch;
        else
            return null;
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
            if (pair.Value == obj)
            {
                return pair.Key; // Return the ID associated with the object
            }
        }

        return 0; // Return 0 if the object is not found
    }
}
