using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;

public class DroneBlock : MonoBehaviour
{

    public Vector3 scanBoxSize = new Vector3(1.1f,1.1f,1.1f);
    public Vector3 scanBoxOffset = Vector3.zero;
    public Vector3 gridSize = Vector3.one;
    public float mass = 25;
    public float health = 10;
    public float cost = 25;
    
    public Transform connectionPoint;

    public UnityEvent onInit; // called when all parents have been reparented to the core
    public UnityEvent postAdoptionInit; // called when all parents and children have been reparented to the core

    [HideInInspector]
    public IPlaceable blockIdentity;

    [HideInInspector]
    public DroneController controller;

    //public bool isJoint;
    
    public BlockSaveMetaData meta;
    
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
        
        controller = transform.root.GetComponent<DroneController>();
        controller.curHealth += health;
        
        Rigidbody rb = Utils.FindParentRigidbody(transform);
        if (rb != null)
        {
            rb.mass += mass;
        }
        else
        {
            //controller.movementController.mass += mass;
        }

        
        HashSet<DroneBlock> blocks = ScanForNeighboringDroneBlocks();
        HashSet<DroneBlock> blocksToInit = new HashSet<DroneBlock>();

        foreach (DroneBlock droneBlock in blocks)
        {
            Rigidbody newRb = Utils.FindParentRigidbody(droneBlock.transform);
            
            if (droneBlock.transform.root != transform.root)
            {
                droneBlock.transform.parent = connectionPoint;
                blocksToInit.Add(droneBlock);
            }
            /*else if(newRb != null && newRb != rb && isJoint)
            {
                FixedJoint fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.connectedBody = newRb;
            }*/
        }
        
        foreach (DroneBlock droneBlock in blocksToInit)
        {
            droneBlock.Init();
        }
        
        postAdoptionInit.Invoke();
    }

    HashSet<DroneBlock> ScanForNeighboringDroneBlocks()
    {
        HashSet<DroneBlock> droneBlocks = new HashSet<DroneBlock>();

        // Define the box size for scanning
        Vector3 halfExtents = scanBoxSize / 2f;
    
        // Perform an overlap box check to find all colliders within the box
        Collider[] colliders = Physics.OverlapBox(transform.position + transform.rotation * scanBoxOffset, halfExtents, transform.rotation);

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

    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 halfExtents = scanBoxSize / 2f;
        Gizmos.DrawWireCube(transform.position+ transform.rotation * scanBoxOffset, halfExtents * 2f);
    }*/

    public void TakeDamage(float damage)
    {
        //Debug.Log(gameObject.name + " Took " + damage + " Damage at position " + transform.position);
        if(controller!= null)
            controller.TakeDamage(damage);
    }
}
