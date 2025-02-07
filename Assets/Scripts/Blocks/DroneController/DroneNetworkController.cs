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
    PhysParent physParent;

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
        if(!NetworkManager.Singleton.IsListening)
            return;
        
        if (physParent == null)
            physParent = transform.root.GetComponent<PhysParent>();
        
        SyncValue(health, ref controller.curHealth);
        //SyncValue(maxHealth, ref controller.maxHealth);
        
        SyncValue(energy, ref controller.energy.energy);
        SyncValue(maxEnergy, ref controller.energy.maxEnergy);
        
        SyncValue(boundingRadius, ref controller.boundingSphereRadius);
        
        if (GameManager.Instance.IsOnlineAndClient() && physParent != null)
        {
            if (!hasClientDeployed && physParent.HasBuilt())
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
        controller.ProxyDeploy();

        foreach (IProxyDeploy proxyDeploy in transform.root.GetComponentsInChildren<IProxyDeploy>())
        {
            proxyDeploy.ProxyDeploy();
        }
    }
}
