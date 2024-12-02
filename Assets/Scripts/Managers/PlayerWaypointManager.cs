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
            CreateSpreadWaypoints(Utils.CursorScanPos(), spreadMultiplier);
            
            
        }
    }
    
}