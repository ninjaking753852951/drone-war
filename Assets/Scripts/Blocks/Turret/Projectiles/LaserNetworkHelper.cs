using System;
using Unity.Netcode;
using UnityEngine;
public class LaserNetworkHelper : NetworkBehaviour
{

    LaserBeam laser;
    
    public void Awake()
    {
        laser = GetComponent<LaserBeam>();
    }

    [Rpc(SendTo.Everyone)]
    public void InitVisualRPC(Vector3 end)
    {
        laser.ConfigureVisuals(end);
    }
    
}
