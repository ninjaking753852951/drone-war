using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhysBlock))]
public class DroneBlock : MonoBehaviour
{


    public Vector3 gridSize = Vector3.one;
    
    public float health = 10;
    public float cost = 25;
    
    [HideInInspector]
    public IPlaceable blockIdentity;

    [HideInInspector]
    public DroneController controller;

    public BlockSaveMetaData meta;

    PhysBlock physBlock;
    
    void Awake()
    {
        physBlock = GetComponent<PhysBlock>();
        physBlock.onBuildFinalized.AddListener(FinalizeBuild);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FinalizeBuild()
    {
        controller = transform.root.GetComponentInChildren<DroneController>();
    }
    
    public void TakeDamage(float damage)
    {
        if(controller!= null)
            controller.TakeDamage(damage);
    }
}
