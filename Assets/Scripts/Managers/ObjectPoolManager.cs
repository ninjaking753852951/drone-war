using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityUtils;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public int InitialPoolSize = 10;
    private readonly Dictionary<PooledTypes, Queue<GameObject>> poolDictionary = new Dictionary<PooledTypes, Queue<GameObject>>();

    [FormerlySerializedAs("pooledObjects")]
    public List<PooledObjects> pooledObjectSettings;
    
    [System.Serializable]
    public class PooledObjects
    {
        public PooledTypes type;
        public GameObject prefab;
    }
    
    public enum PooledTypes
    {
        bullet, laser, missile
    }

    private void Start()
    {
        // Initialize pools for all predefined objects
        foreach (var poolObject in pooledObjectSettings)
        {
            CreatePool(poolObject.type);
        }
    }
    
    public GameObject RequestObject(PooledTypes type, Vector3 pos)
    {
        // Find the correct prefab for the type
        GameObject prefab = pooledObjectSettings.Find(po => po.type == type)?.prefab;
        
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for type: {type}");
            return null;
        }

        // Ensure pool exists
        if (!poolDictionary.ContainsKey(type))
        {
            CreatePool(type);
        }

        Queue<GameObject> pool = poolDictionary[type];

        // Return existing object or expand pool
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);

            obj.transform.position = pos;
            
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkObject networkObject = obj.GetComponent<NetworkObject>();
                if (networkObject != null && !networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }
            }
            
            return obj;
        }
        else
        {
            return ExpandPool(type, pos);
        }
    }

    public void ReturnObject(GameObject obj, PooledTypes type)
    {

        if (NetworkManager.Singleton.IsListening)
        {
            NetworkObject networkObject = obj.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                networkObject.Despawn(false);
            }
        }
        else
        {
            Utils.RemoveNetworkComponents(obj);
        }

        obj.SetActive(false);
        poolDictionary[type].Enqueue(obj);
    }

    private void CreatePool(PooledTypes type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            poolDictionary.Add(type, new Queue<GameObject>()); 

            // Create initial pool
            for (int i = 0; i < InitialPoolSize; i++)
            {
                ExpandPool(type, Vector3.zero);
            }
        }
    }

    private GameObject ExpandPool(PooledTypes type, Vector3 pos)
    {
        PooledObjects objSettings = pooledObjectSettings.Find(po => po.type == type);
    
        if (objSettings.prefab == null)
        {
            Debug.LogError($"No prefab found for type: {type}");
            return null;
        }

        GameObject newObject = Instantiate(objSettings.prefab);
        //newObject.SetActive(false);
        
        newObject.transform.position = pos;

        if (NetworkManager.Singleton.IsListening)
        {
            NetworkObject networkObject = newObject.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Always despawn and set inactive, regardless of current spawn state
                networkObject.Spawn();
            }
        }
        else
        {
            Utils.RemoveNetworkComponents(newObject);
        }

        //poolDictionary[type].Enqueue(newObject);
        return newObject;
    }
}