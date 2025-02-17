using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class AIDroneSpawner : DroneSpawner
{
    public float spawnRate;

    public int2 spawnSlots;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        
        if(GameManager.Instance.currentGameMode != GameMode.Battle)
            return;
        
        InvokeRepeating(nameof(SpawnRandomMachine), 2, spawnRate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SpawnRandomMachine()
    {
        SpawnMachine(Random.Range(spawnSlots.x,spawnSlots.y +1) - 100);
    }
}
