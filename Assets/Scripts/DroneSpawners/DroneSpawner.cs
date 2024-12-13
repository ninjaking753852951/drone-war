using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class DroneSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    DroneController controller;

    public float scanRadius;

    //public int teamID;
    public MatchManager.TeamData teamData;
    
    

    public void SpawnMachine(int id)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            if(!NetworkManager.Singleton.IsServer)
                return;
        }

        Vector3 scanPos = SafeSpawnPosition();
        
        MachineSaveData machineData = MachineSaveLoadManager.Instance.LoadMachine(id);
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
        controller = machineData.Spawn(offset: scanPos, eulerRot: transform.rotation.eulerAngles, teamID:MatchManager.Instance.TeamID(teamData));

        StartCoroutine(SpawnMachineCoroutine());
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
                DroneBlock block = collider.transform.root.GetComponent<DroneBlock>();
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

    IEnumerator SpawnMachineCoroutine()
    {

        yield return new WaitForFixedUpdate();
        
        //yield return new WaitForEndOfFrame();
        controller.Deploy();
        //controller.transform.root.position = spawnPoint.position + Vector3.up * 5;
    }
}
