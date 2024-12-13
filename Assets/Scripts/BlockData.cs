using Interfaces;using Unity.Netcode;
using UnityEngine;
[System.Serializable]
public class BlockData : IPlaceable
{
    public bool isCore;
    public GameObject prefab;
    public BuildingManagerUI.PlaceableCategories category;

    public string PlaceableName()
    {
        return prefab.name;
    }
    public float Cost()
    {
        return prefab.GetComponent<DroneBlock>().cost;
    }
    public GameObject Spawn(Vector3 pos, Quaternion rot)
    {
        GameObject blockClone = GameObject.Instantiate(prefab, pos, rot);
        blockClone.GetComponent<DroneBlock>().blockIdentity = this;
        
        if (NetworkManager.Singleton.IsListening)
        {
            NetworkObject netObj = blockClone.GetComponent<NetworkObject>();
                    
            if(netObj != null)
                netObj.Spawn();
        }
        else
        {
            Utils.RemoveNetworkComponents(blockClone);
        }
                
        return blockClone;
    }
    public Sprite Thumbnail()
    {
        Vector3 offset = Vector3.up * 1000 * -1;
        GameObject obj = Spawn(offset, Quaternion.Euler(-35, 35, 0));
        return ThumbnailGenerator.Instance.GenerateThumbnail(obj);
    }
    public BuildingManagerUI.PlaceableCategories Category()
    {
        return category;
    }
}
