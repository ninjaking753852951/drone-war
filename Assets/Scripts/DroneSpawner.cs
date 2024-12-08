using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DroneSpawner : MonoBehaviour
{
    public Transform spawnPoint;

    DroneController controller;

    public float scanRadius;

    public int teamID;
    

    protected void SpawnMachine(int id)
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
        
        MachineSaveLoadManager.MachineSaveData machineData = MachineSaveLoadManager.Instance.LoadMachine(id);
        if(machineData == null)
            return;
        
        float machineCost = machineData.totalCost;

        if (MatchManager.Instance.Team(teamID).CanAfford(machineCost))
        {
            MatchManager.Instance.Team(teamID).DeductMoney(machineCost);
        }
        else
        {
            return;
        }
        controller = machineData.Spawn(scanPos);
        controller.curTeam = teamID;

        StartCoroutine(SpawnMachineCoroutine());
    }

    IEnumerator SpawnMachineCoroutine()
    {

        yield return new WaitForFixedUpdate();
        
        //yield return new WaitForEndOfFrame();
        controller.Deploy(true);
        //controller.transform.root.position = spawnPoint.position + Vector3.up * 5;
    }
}
