using System;
using System.Collections.Generic;
using Unity.Netcode;
public abstract class NetworkHelperBase : NetworkBehaviour
{
    protected void SyncValue<T>(NetworkVariable<T> networkVariable, ref T localValue)
    {
        if(!NetworkManager.Singleton.IsListening)
            return;
        
        if (NetworkManager.Singleton.IsServer)
        {
            networkVariable.Value = localValue;
        }
        else
        {
            localValue = networkVariable.Value;
        }
    }
    
    protected void SyncList<T>(NetworkList<T> networkList, ref List<T> localList)where T : unmanaged, IEquatable<T>
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // Sync from local list to NetworkList
            networkList.Clear();
            foreach (var item in localList)
            {
                networkList.Add(item);
            }
        }
        else
        {
            // Sync from NetworkList to local list
            localList.Clear();
            foreach (var item in networkList)
            {
                localList.Add(item);
            }
        }
    }
}
