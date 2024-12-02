using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDroneSpawner : DroneSpawner
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnGUI()
    {
        if(GameManager.Instance.currentGameMode != GameMode.Battle)
            return;
        
        GUILayout.BeginArea(new Rect(10, 25, 200, 300)); // Adjust area size as needed
        GUILayout.Label("Machine Slots");

        for (int i = 1; i <= 10; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Slot {i}", GUILayout.Width(50));

            if (GUILayout.Button("Spawn", GUILayout.Width(60)))
            {
                SpawnMachine(i);
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }
}
