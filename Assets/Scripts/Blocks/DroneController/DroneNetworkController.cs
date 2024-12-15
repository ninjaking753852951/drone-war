using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
public class DroneNetworkController : NetworkBehaviour
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

    void Update()
    {
        if (GameManager.Instance.IsOnlineAndClient())
        {
            if (!hasClientDeployed && blockCount.Value == GetComponentsInChildren<DroneBlock>().Length)
            {
                ProxyDeploy();
            }
            
            controller.curHealth = health.Value;
            controller.maxHealth = maxHealth.Value;
            
            controller.energy.energy = energy.Value;
            controller.energy.maxEnergy = maxEnergy.Value;

            controller.boundingSphereRadius = boundingRadius.Value;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            health.Value = controller.curHealth;
            maxHealth.Value = controller.maxHealth;

            energy.Value = controller.energy.energy;
            maxEnergy.Value = controller.energy.maxEnergy;

            boundingRadius.Value = controller.boundingSphereRadius;
        }
    }

    void ProxyDeploy()
    {
        hasClientDeployed = true;
        controller.curTeam = curTeam.Value;
        controller.ClientDeploy();

        foreach (IProxyDeploy proxyDeploy in GetComponentsInChildren<IProxyDeploy>())
        {
            proxyDeploy.ProxyDeploy();
        }
    }
    
    /*IEnumerator DelayedProxyDeploy()
    {

        yield return new WaitForEndOfFrame();
        ProxyDeploy();
    }*/
}
