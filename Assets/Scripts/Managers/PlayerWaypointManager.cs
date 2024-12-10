using System.Collections.Generic;
using System.Linq;
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
            List<DroneController> playerDrones = Utils.DronesFromTeam(SelectionManager.Instance.selectedDrones.ToList(), MatchManager.Instance.playerID);
            float spread = FindBiggestRadius(playerDrones) * 2;// ASSUMING WORST CASE SCENARIO THE TWO BIGGEST ARE NEXT TO EACHOTHER

            List<Vector3> posOffsets = spreadPattern.GenerateSpread(playerDrones.Count, spread * spreadMultiplier);
            Vector3 pos = Utils.CursorScanPos();
            
            for (int i = 0; i < playerDrones.Count; i++)
            {
                DroneController drone = playerDrones[i];
    
                ulong droneID = MachineInstanceManager.Instance.FetchID(drone.gameObject);
                CommandManager.Instance.AddCommand(new CommandManager.Command(droneID,pos + posOffsets[i]));
            }
            //CreateSpreadWaypoints(Utils.CursorScanPos(), playerDrones, spreadMultiplier);
        }
    }
    
}