using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{

    public GameObject waypointPrefab;

    public List<Waypoint> waypoints = new List<Waypoint>();
    
    [System.Serializable]
    public class Waypoint
    {
        public GameObject waypointMarker;
        public List<DroneController> followers;
        public Waypoint(GameObject waypointMarker, List<DroneController> followers)
        {
            this.waypointMarker = waypointMarker;
            this.followers = followers;
        }
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(BuildingManager.Instance.isBuilding)
            return;
        
        
        if(Input.GetMouseButtonDown(1) && SelectionManager.Instance.selectedDrones.Count > 0)
            CreateWaypoint();
        
    }

    void CreateWaypoint()
    {
        Vector3 targetPos = Utils.CursorScanPos();
        
        if(targetPos == Vector3.zero)
            return;

        List<DroneController> drones = SelectionManager.Instance.selectedDrones.ToList();
     
        RemoveDronesFromWayPoints(drones);
        
        GameObject waypointClone = Instantiate(waypointPrefab, targetPos, quaternion.identity);

        foreach (var drone in drones)
        {
            drone.targetDestination = waypointClone.transform;
        }


        
        waypoints.Add(new Waypoint(waypointClone, drones));
        
    }

    void RemoveDronesFromWayPoints(List<DroneController> dronesToRemove)
    {
        // Iterate backwards to allow safe removal of waypoints from the list
        for (int i = waypoints.Count - 1; i >= 0; i--)
        {
            Waypoint waypoint = waypoints[i];
            
            if(waypoint == null)
                continue;

            // Remove each drone in dronesToRemove from the waypoint's followers
            for (int j = waypoint.followers.Count - 1; j >= 0; j--)
            {
                DroneController drone = waypoint.followers[j];

                if (dronesToRemove.Contains(drone))
                    waypoint.followers.Remove(drone);
            }

            // If there are no followers left, destroy the waypoint and remove it from the list
            if (waypoint.followers.Count == 0)
            {
                Destroy(waypoint.waypointMarker); // Destroy the waypoint GameObject
                waypoints.RemoveAt(i); // Remove the waypoint from the list
            }
        }
    }

}
