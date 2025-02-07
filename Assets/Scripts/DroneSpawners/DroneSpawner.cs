using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class DroneSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    DroneController controller;

    public float scanRadius;

    [HideInInspector]
    public int teamID;
    public MatchManager.TeamData teamData;

    protected virtual void Awake()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        //NetworkManager.Singleton.OnServerStarted += () => netObj.Spawn();
    }

    protected virtual void Start()
    {
        teamID = MatchManager.Instance.RegisterTeam(this);
    }


    public void SpawnMachine(int id)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            if(!NetworkManager.Singleton.IsServer)
                return;
        }

        Vector3 scanPos = SafeSpawnPosition();
        
        MachineSaveData machineData = MachineLibraryManager.Instance.FetchMachine(id);
        if(machineData == null)
            return;
        
        float machineCost = machineData.totalCost;

        if (teamData.CanAfford(machineCost))
        {
            teamData.DeductMoney(machineCost);
        }
        else
        {
            return;
        }
        controller = machineData.Spawn(offset: scanPos, eulerRot: transform.rotation.eulerAngles, teamID:teamID, deploy:true, network:true);

        //StartCoroutine(SpawnMachineCoroutine());
    }

    Vector3 SafeSpawnPosition()
    {
        bool isClear = false;
        int safety = 100;
        Vector3 scanPos = spawnPoint.position;
        while (!isClear && safety > 0)
        {
            isClear = true;
            safety--;
            
            Collider[] colliders = Physics.OverlapSphere(scanPos, scanRadius);

            foreach (var collider in colliders)
            {
                PhysParent block = collider.transform.root.GetComponent<PhysParent>();
                if (block != null)
                {
                    isClear = false;
                    break;
                }
            }

            if (!isClear)
            {
                scanPos -= transform.forward * scanRadius * 2;
            }
        }
        return scanPos;
    }

    /*IEnumerator SpawnMachineCoroutine()
    {

        yield return new WaitForFixedUpdate();
        
        controller.Deploy();

    }*/
}
