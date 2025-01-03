using UnityEngine;

public abstract class PhysJointBuilder : ScriptableObject
{
    public abstract Joint Build(Vector3 anchorPoint, Quaternion rot, PhysCluster originCluster, PhysCluster connectedCluster);
}