using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class VFXManager : NetworkBehaviour
{
    public static VFXManager instance { get; private set; }

    [SerializeField] private List<VFXData> vfxList = new List<VFXData>();

    private void Awake()
    {

        instance = this;
    }

    public void Spawn(VFXData effect, Vector3 position, Quaternion rotation, bool spawnOnClients = true)
    {
        if (!vfxList.Contains(effect))
        {
            Debug.LogError("Effect not registered in VFX list!");
            return;
        }

        int effectIndex = vfxList.IndexOf(effect);

        // Spawn locally
        GameObject vfx = Instantiate(effect.prefab, position, rotation);
        Destroy(vfx, effect.lifetime);

        // If networked spawn is requested
        if (spawnOnClients && IsServer)
        {
            /*ulong parentID = 0;
            NetworkObject parentNetObj
            if(parent != null)
                parentID = */
            
            SpawnVFXRpc(effectIndex, position, rotation);
        }
    }

    [Rpc(SendTo.NotServer)]
    private void SpawnVFXRpc(int effectIndex, Vector3 position, Quaternion rotation)
    {
        if (effectIndex < 0 || effectIndex >= vfxList.Count)
        {
            Debug.LogError("Invalid effect index received!");
            return;
        }

        /*Transform parent = null;
        if (parentId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentId, out NetworkObject parentNetworkObject))
        {
            parent = parentNetworkObject.transform;
        }*/

        VFXData effect = vfxList[effectIndex];
        GameObject vfx = Instantiate(effect.prefab, position, rotation);
        Destroy(vfx, effect.lifetime);
    }
}