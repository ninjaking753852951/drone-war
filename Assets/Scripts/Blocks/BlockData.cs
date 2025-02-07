using System;
using System.Collections.Generic;
using Interfaces;using Unity.Netcode;
using UnityEngine;
[System.Serializable]
public class BlockData : IPlaceable
{
    public GameObject prefab;
    public BlockType category;

    public string PlaceableName()
    {
        return prefab.name;
    }
    public float Cost() => Stats().QueryStat(Stat.Cost);
    
    public GameObject Spawn(Vector3 pos, Quaternion rot, bool network = true)
    {
        GameObject blockClone = GameObject.Instantiate(prefab, pos, rot);
        blockClone.GetComponent<DroneBlock>().blockIdentity = this;
        
        if (NetworkManager.Singleton.IsListening && network)
        {
            NetworkObject netObj = blockClone.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.SpawnWithObservers = false;
                netObj.Spawn();   
                if(netObj.IsSpawned)
                    Debug.Log("BLOCK NOT INSTA SPAWNED");
            }
            
            //netObj.SpawnWithObservers()
        }
        else
        {
            List<Type> exceptions = new List<Type>();
            exceptions.Add(typeof(DroneController));
            exceptions.Add(typeof(TurretCoreController));
            
            Utils.RemoveNetworkComponents(blockClone, exceptions);
        }
                
        return blockClone;
    }
    public Sprite Thumbnail()
    {
        Vector3 offset = Vector3.up * 1000 * -1;
        GameObject obj = Spawn(offset, Quaternion.Euler(-35, 35, 0));
        return ThumbnailManager.Instance.GenerateThumbnail(obj);
    }
    public BlockType Category()
    {
        return category;
    }
    public BlockStats Stats() => prefab.GetComponent<DroneBlock>().stats;
}
