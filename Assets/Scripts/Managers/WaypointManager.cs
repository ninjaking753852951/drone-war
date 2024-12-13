using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaypointManager : UnityUtils.Singleton<WaypointManager>
{

    public GameObject waypointPrefab;

    List<Waypoint> waypoints = new List<Waypoint>();

    public ISpreadPattern spreadPattern = new CircleSpreadPattern();
    
    class Waypoint
    {
        public GameObject waypointMarker;
        List<DroneController> followers;
        
        public Waypoint(WaypointManager waypointManager, Vector3 pos, List<DroneController> followers)
        {
            waypointMarker = Instantiate(waypointManager.waypointPrefab, pos, quaternion.identity);
            RegisterFollower(followers);
        }
        public Waypoint(WaypointManager waypointManager, Vector3 pos, DroneController follower)
        {
            waypointMarker = Instantiate(waypointManager.waypointPrefab, pos, quaternion.identity);
            if(NetworkManager.Singleton.IsServer)
                waypointMarker.GetComponent<NetworkObject>().Spawn();
            
            RegisterFollower(follower);
        }

        void RegisterFollower(DroneController follower)
        {
            followers ??= new List<DroneController>();
            
            followers.Add(follower);
            follower.SetDestination(waypointMarker.transform.position);
        }

        void RegisterFollower(List<DroneController> followers)
        {
            foreach (DroneController follower in followers)
            {
                RegisterFollower(follower);
            }
        }
        
        public bool DeregisterFollower(DroneController follower)
        {
            followers.Remove(follower);
            
            if(followers.Count == 0 && waypointMarker != null)
                Destroy(waypointMarker);
            
            return followers.Count == 0;
        }

        public bool DeregisterFollower(List<DroneController> followers)
        {
            bool isEmpty = false;
            for (int i = followers.Count - 1; i >= 0; i--)
            {
                DroneController follower = followers[i];
                isEmpty = isEmpty || DeregisterFollower(follower);
            }
            return isEmpty;
        }
    }

    void Start()
    {
        GameManager.Instance.onEnterBuildMode.AddListener(ClearWaypoints);
    }

    void ClearWaypoints()
    {
        if(waypoints == null)
            return;
        
        foreach (Waypoint waypoint in waypoints)
        {
            if(waypoint.waypointMarker != null)
                Destroy(waypoint.waypointMarker);
        }
        waypoints.Clear();
    }

    protected void CreateSpreadWaypoints(Vector3 pos, List<DroneController> drones, float spreadMultiplier)
    {
        if(pos == Vector3.zero)
            Debug.LogWarning("Waypoint created at null position (zero)");
        
        if(drones.Count == 0)
            return;
        
        RemoveDronesFromWayPoints(drones);
        float spread = FindBiggestRadius(drones) * 2;// ASSUMING WORST CASE SCENARIO THE TWO BIGGEST ARE NEXT TO EACHOTHER

        List<Vector3> posOffsets = spreadPattern.GenerateSpread(drones.Count, spread * spreadMultiplier);

        for (var i = 0; i < drones.Count; i++)
        {
            Waypoint waypoint = new Waypoint(this, pos + posOffsets[i], drones[i]);
            waypoints.Add(waypoint);
        }
    }

    public void CreateAndAssignToWaypoint(Vector3 pos, DroneController controller)
    {
        RemoveDroneFromWayPoints(controller);
        waypoints.Add(new Waypoint(this, pos, controller));
    }

    protected float FindBiggestRadius(List<DroneController> drones)
    {
        float biggestRadius = 0;
        
        foreach (var drone in drones)
        {
            biggestRadius = Mathf.Max(biggestRadius, drone.boundingSphereRadius);
        }

        return biggestRadius;
    }

    void RemoveDronesFromWayPoints(List<DroneController> dronesToRemove)
    {
        foreach (DroneController drone in dronesToRemove)
        {
            RemoveDroneFromWayPoints(drone);
        }
    }
    
    void RemoveDroneFromWayPoints(DroneController droneToRemove)
    {
        // Iterate backwards to allow safe removal of waypoints from the list
        for (int i = waypoints.Count - 1; i >= 0; i--)
        {
            Waypoint waypoint = waypoints[i];

            if (waypoint == null || waypoint.DeregisterFollower(droneToRemove))
            {
                waypoints.RemoveAt(i);
            }
        }
    }

}


public interface ISpreadPattern
{
    public List<Vector3> GenerateSpread(float points, float spread);
}

public class CircleSpreadPattern : ISpreadPattern
{
    public List<Vector3> GenerateSpread(float points, float spread)
    {
        List<Vector3> positions = new List<Vector3>();
        
        // Validate input
        if (points <= 0 || spread <= 0)
            return positions;

        if (points == 1)
        {
            positions.Add(Vector3.zero);
            return positions;
        }

        // Calculate angle between points
        float angleStep = 360f / points;

        for (int i = 0; i < points; i++)
        {
            // Calculate the position of each point
            float angleInRadians = Mathf.Deg2Rad * angleStep * i;
            float x = Mathf.Cos(angleInRadians) * spread;
            float z = Mathf.Sin(angleInRadians) * spread;

            positions.Add(new Vector3(x, 0, z)); // Assuming a flat circle on the XZ plane
        }

        return positions;
    }
}

public class SquareGridSpreadPattern : ISpreadPattern
{
    public List<Vector3> GenerateSpread(float points, float spread)
    {
        List<Vector3> positions = new List<Vector3>();

        if (points <= 0 || spread <= 0)
            return positions;

        // Determine grid size (square root of points, rounded up)
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(points));

        // Start position (centered grid)
        float offset = (gridSize - 1) * spread / 2;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Stop adding positions once we've reached the desired number of points
                if (positions.Count >= points)
                    return positions;

                // Calculate the position of each point on the grid
                float x = i * spread - offset;
                float z = j * spread - offset;

                positions.Add(new Vector3(x, 0, z)); // Assuming a flat grid on the XZ plane
            }
        }

        return positions;
    }
}
