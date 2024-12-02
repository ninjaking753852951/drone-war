using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDroneSpawner : DroneSpawner
{
    public float spawnRate;
    
    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.Instance.currentGameMode != GameMode.Battle)
            return;
        
        InvokeRepeating(nameof(SpawnRandomMachine), 1, spawnRate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnRandomMachine()
    {
        SpawnMachine(Random.Range(1,4));
    }
}
