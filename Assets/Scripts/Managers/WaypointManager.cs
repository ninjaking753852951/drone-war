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

    protected ISpreadPattern spreadPattern = new CircleSpreadPattern();
    
    public class Waypoint
    {
        public GameObject waypointMarker;
        DroneController follower;
        
        public Waypoint(WaypointManager waypointManager, Vector3 pos, DroneController follower)
        {
            waypointMarker = Instantiate(waypointManager.waypointPrefab, pos, quaternion.identity);
            if(NetworkManager.Singleton.IsServer)
                waypointMarker.GetComponent<NetworkObject>().Spawn();
            
            RegisterFollower(follower, waypointManager);
        }

        void RegisterFollower(DroneController follower, WaypointManager waypointManager)
        {
            this.follower = follower;

            //follower.onDroneDestroyed.AddListener(waypointManager.RemoveDroneFromWayPoints);
            
            follower.AddWaypoint(this);
        }
        
        public bool DeregisterFollower(DroneController follower)
        {
            if (this.follower == follower)
            {
                Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                waypointMarker.GetComponent<NetworkObject>().Despawn();  
            }
            else
            {
                Destroy(waypointMarker);
            }
        }
    }

    protected virtual void Start()
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
                waypoint.Dispose();
        }
        waypoints.Clear();
    }


    public void CreateAndSetWaypoint(Vector3 pos, DroneController controller)
    {
        controller.ClearWaypoints();
        waypoints.Add(new Waypoint(this, pos, controller));
    }
    
    public void CreateAndQueueWaypoint(Vector3 pos, DroneController controller)
    {
        waypoints.Add(new Waypoint(this, pos, controller));
    }

    protected float FindBiggestRadius(List<DroneController> drones)
    {
        return drones.Max(drone => drone.boundingSphereRadius);
    }
    
    /*void RemoveDroneFromWayPoints(DroneController droneToRemove)
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
    }*/

    public void DisposeWaypoint(Waypoint waypoint)
    {
        waypoint.Dispose();
        waypoints.Remove(waypoint);
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

        // Calculate the radius of the circle so that the distance between points equals `spread`
        float circumference = spread * points * 2;
        float radius = circumference / (2 * Mathf.PI);

        // Calculate angle between points
        float angleStep = 360f / points;

        for (int i = 0; i < points; i++)
        {
            // Calculate the position of each point
            float angleInRadians = Mathf.Deg2Rad * angleStep * i;
            float x = Mathf.Cos(angleInRadians) * radius;
            float z = Mathf.Sin(angleInRadians) * radius;

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
