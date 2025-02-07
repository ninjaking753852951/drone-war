using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SFXManager : NetworkSingleton<SFXManager>
{

    public float masterVolume = 0.5f;
    
    [FormerlySerializedAs("vfxList")]
    [SerializeField] private List<SFXData> sfxList = new List<SFXData>();

    public GameObject sfxPrefab;

    public void Play(SFXData effect, Vector3 position = default, bool spawnOnClients = false)
    {
        if (!sfxList.Contains(effect))
        {
            Debug.LogError("Effect not registered in SFX list!");
            return;
        }

        int effectIndex = sfxList.IndexOf(effect);

        AudioClip clip = effect.Clip();
        
        // Spawn locally
        GameObject sfxClone = Instantiate(sfxPrefab, position, quaternion.identity);

        AudioSource source = sfxClone.GetComponent<AudioSource>();
        source.clip = clip;
        source.volume = masterVolume * effect.volume;
        source.pitch = effect.basePitch + (Random.value - 0.5f) * effect.pitchVariance;
        source.spatialBlend = effect.spatialBlend;
        source.Play();
        Destroy(sfxClone, clip.length + 1); // plus an extra second just incase

        // If networked spawn is requested
        if (spawnOnClients && IsServer)
        {
            SpawnVFXRpc(effectIndex, position);
        }
    }

    [Rpc(SendTo.NotServer)]
    private void SpawnVFXRpc(int effectIndex, Vector3 position)
    {
        if (effectIndex < 0 || effectIndex >= sfxList.Count)
        {
            Debug.LogError("Invalid effect index received!");
            return;
        }

        /*Transform parent = null;
        if (parentId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentId, out NetworkObject parentNetworkObject))
        {
            parent = parentNetworkObject.transform;
        }*/
        
        SFXData effect = sfxList[effectIndex];
        
        Play(effect, position, false);
    }
}