using UnityEngine;

public abstract class PhysJointBuilder : ScriptableObject
{
    public abstract Joint Build(Vector3 anchorPoint, PhysCluster originCluster, PhysCluster connectedCluster);
}