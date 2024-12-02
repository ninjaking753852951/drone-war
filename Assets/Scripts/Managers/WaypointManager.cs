using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaypointManager : MonoBehaviour
{

    public GameObject waypointPrefab;

    public List<Waypoint> waypoints = new List<Waypoint>();

    ISpreadPattern spreadPattern = new CircleSpreadPattern();
    
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
        public Waypoint(GameObject waypointMarker, DroneController follower)
        {
            this.waypointMarker = waypointMarker;
            List<DroneController> _followers = new List<DroneController>();
            _followers.Add(follower);
            this.followers = _followers;
        }
    }
    

    protected Waypoint CreateWaypoint(Vector3 pos)
    {
        if(pos == Vector3.zero)
            Debug.LogWarning("Waypoint created at null position (zero)");

        List<DroneController> drones = SelectionManager.Instance.selectedDrones.ToList();
     
        RemoveDronesFromWayPoints(drones);
        
        GameObject waypointClone = Instantiate(waypointPrefab, pos, quaternion.identity);

        foreach (var drone in drones)
        {
            drone.SetDestination(waypointClone.transform);
        }


        Waypoint waypoint = new Waypoint(waypointClone, drones);
        
        waypoints.Add(waypoint);

        return new Waypoint(waypointClone, drones);

    }
    
    protected void CreateSpreadWaypoints(Vector3 pos, float spreadMultiplier)
    {
        if(pos == Vector3.zero)
            Debug.LogWarning("Waypoint created at null position (zero)");

        List<DroneController> drones = SelectionManager.Instance.selectedDrones.ToList();
     
        RemoveDronesFromWayPoints(drones);
        float spread = FindBiggestRadius(drones) * 2;// ASSUMING WORST CASE SCENARIO THE TWO BIGGEST ARE NEXT TO EACHOTHER

        List<Vector3> posOffsets = spreadPattern.GenerateSpread(drones.Count, spread * spreadMultiplier);

        for (var i = 0; i < drones.Count; i++)
        {
            var drone = drones[i];
            Vector2 randomOffset = Random.insideUnitCircle * spreadMultiplier;
            GameObject waypointClone = Instantiate(waypointPrefab, pos + posOffsets[i],
                quaternion.identity);
            drone.SetDestination(waypointClone.transform);
            Waypoint waypoint = new Waypoint(waypointClone, drones);

            waypoints.Add(waypoint);
        }
    }

    float FindBiggestRadius(List<DroneController> drones)
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


interface ISpreadPattern
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
