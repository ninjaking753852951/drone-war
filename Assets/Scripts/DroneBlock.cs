using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DroneBlock : MonoBehaviour
{

    public float scanBoxSize = 1.1f;

    public Transform connectionPoint;

    public UnityEvent onInit;

    [HideInInspector]
    public BlockData blockIdentity;
    
    
    void Awake()
    {
        if (connectionPoint == null)
            connectionPoint = transform;
    }

    void Start()
    {

    }

    public void Init()
    {
        
                
        onInit.Invoke();
        
        HashSet<DroneBlock> blocks = ScanForNeighboringDroneBlocks();
        HashSet<DroneBlock> blocksToInit = new HashSet<DroneBlock>(); // Use a HashSet

        foreach (DroneBlock droneBlock in blocks)
        {
            if (droneBlock.transform.root != transform.root)
            {
                droneBlock.transform.parent = connectionPoint;
                blocksToInit.Add(droneBlock); // Mark for removal after the loop
            }
        }

        // Remove blocks that were marked
        foreach (DroneBlock droneBlock in blocksToInit)
        {
            droneBlock.Init();
        }
    }

    HashSet<DroneBlock> ScanForNeighboringDroneBlocks()
    {
        HashSet<DroneBlock> droneBlocks = new HashSet<DroneBlock>();

        // Define the box size for scanning
        Vector3 halfExtents = Vector3.one * scanBoxSize / 2f;
    
        // Perform an overlap box check to find all colliders within the box
        Collider[] colliders = Physics.OverlapBox(transform.position, halfExtents, transform.rotation);

        // Loop through each collider and check if it has a DroneBlock component
        foreach (Collider collider in colliders)
        {
            DroneBlock droneBlock = collider.GetComponent<DroneBlock>();
            if (droneBlock != null && droneBlock != this) // Exclude self
            {
                droneBlocks.Add(droneBlock);
            }
        }

        return droneBlocks;
    }    

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 halfExtents = Vector3.one * scanBoxSize / 2f;
        Gizmos.DrawWireCube(transform.position, halfExtents * 2f);
    }

}
