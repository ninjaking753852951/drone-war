using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PhysJointPhysBlock : PhysBlock
{
    
    public PhysCluster connectedCluster { get; private set; }
    
    public PhysJointBuilder jointBuilder;


    public Joint joint { get; private set; }
    
    
    public override void Init(PhysCluster originCluster)
    {
        originCluster.RegisterBlock(this);
    }

    public void InitPhysJoint()
    {
        Vector3 relativeAnchor = originCluster.transform.InverseTransformPoint(transform.position);

        List<PhysBlock> connectedBlocks = ScanForNeighboringDroneBlocks().ToList();
        if(connectedBlocks.Count <= 0)
            return;
        
        if(connectedBlocks[0].originCluster == originCluster)
            return;

        connectedCluster = connectedBlocks[0].originCluster; 
        
        joint = jointBuilder.Build(relativeAnchor,originCluster, connectedCluster);
    }
    
}
