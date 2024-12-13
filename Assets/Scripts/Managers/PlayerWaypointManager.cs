using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerWaypointManager : WaypointManager
{

    public float spreadMultiplier = 1;

    void Update()
    {
        if(GameManager.Instance.currentGameMode == GameMode.Build)
            return;


        // Create and assign waypoint on click
        if (Input.GetMouseButtonDown(1) && SelectionManager.Instance.selectedDrones.Count > 0)
        {
            int teamID = 0;
            if (GameManager.Instance.IsOnlineAndClient())
            {
                teamID = (int)NetworkManager.Singleton.LocalClientId;
            }
            
            List<DroneController> playerDrones = Utils.DronesFromTeam(SelectionManager.Instance.selectedDrones.ToList(), teamID);
            int droneCount = playerDrones.Count;
            float spread = FindBiggestRadius(playerDrones) * 2;// ASSUMING WORST CASE SCENARIO THE TWO BIGGEST ARE NEXT TO EACHOTHER

            List<Vector3> posOffsets = spreadPattern.GenerateSpread(droneCount, spread * spreadMultiplier);
            Vector3 pos = Utils.CursorScanPos();
            
            for (int i = 0; i < droneCount; i++)
            {
                DroneController drone = playerDrones[i];
    
                ulong droneID = MachineInstanceManager.Instance.FetchID(drone.gameObject);

                CommandManager commandManager = FindObjectOfType<CommandManager>();
                if (commandManager != null)
                {
                    if (GameManager.Instance.IsOnlineAndClient())
                    {
                        commandManager.AddCommandRPC(new CommandManager.Command(NetworkManager.Singleton.LocalClientId, droneID,pos + posOffsets[i]).GenerateData());   
                    }
                    else
                    {
                        commandManager.AddCommand(new CommandManager.Command(0,droneID,pos + posOffsets[i]));
                    }
                }
            }
        }
    }
    
}