using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
public class DroneNetworkController : NetworkHelperBase
{
    [HideInInspector]
    public NetworkVariable<int> blockCount;
    public NetworkVariable<int> curTeam;
    bool hasClientDeployed = false;

    public NetworkVariable<float> health;
    public NetworkVariable<float> maxHealth;

    public NetworkVariable<float> energy;
    public NetworkVariable<float> maxEnergy;

    public NetworkVariable<float> boundingRadius;
    
    DroneController controller;

    void Awake()
    {
        controller = GetComponent<DroneController>();
    }

    /*public override void OnNetworkSpawn()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            ProxyDeploy();
        }
    }*/
    
    void Update()
    {
        SyncValue(health, ref controller.curHealth);
        SyncValue(maxHealth, ref controller.maxHealth);
        
        SyncValue(energy, ref controller.energy.energy);
        SyncValue(maxEnergy, ref controller.energy.maxEnergy);
        
        SyncValue(boundingRadius, ref controller.boundingSphereRadius);
        
        if (GameManager.Instance.IsOnlineAndClient())
        {
            if (!hasClientDeployed && blockCount.Value == GetComponentsInChildren<DroneBlock>().Length)
            {
                ProxyDeploy();
            }
        }
    }

    void ProxyDeploy()
    {
        Debug.Log("Proxy Deploy");
        hasClientDeployed = true;
        controller.curTeam = curTeam.Value;
        controller.ClientDeploy();

        foreach (IProxyDeploy proxyDeploy in GetComponentsInChildren<IProxyDeploy>())
        {
            proxyDeploy.ProxyDeploy();
        }
    }
}
