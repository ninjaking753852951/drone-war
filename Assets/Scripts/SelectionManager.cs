using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

public class SelectionManager : Singleton<SelectionManager>
{
    public List<DroneController> selectedDrones = new List<DroneController>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            SelectObject();
    }

    void SelectObject()
    {
        if(BuildingManager.Instance.isBuilding)
            return;
        
        Transform hitTransform = Utils.CursorScan();

        if (hitTransform != null)
        {
            // Check if the root object has a DroneController component
            DroneController drone = hitTransform.root.GetComponent<DroneController>();

            if (drone != null)
            {
                // If it has a DroneController, add it to the list if it's not already selected
                if (!selectedDrones.Contains(drone))
                {
                    drone.outline.enabled = true;
                    selectedDrones.Add(drone);
                    Debug.Log("Drone selected: " + drone.name);
                }
            }
            else
            {
                // Clear the selected drones if no drone is clicked
                ClearSelectedDrones();
            }
        }
        else
        {
            // Clear selected drones if clicking on empty space
            ClearSelectedDrones();
        }
    }

    void ClearSelectedDrones()
    {
        foreach (DroneController drone in selectedDrones)
        {
            drone.outline.enabled = false;
        }
        selectedDrones.Clear();
        Debug.Log("Cleared selected drones.");
    }
}
