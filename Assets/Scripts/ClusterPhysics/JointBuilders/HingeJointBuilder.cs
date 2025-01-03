using UnityEngine;

[CreateAssetMenu(fileName = "HingeJoint", menuName = "ScriptableObjects/Physics/HingeJoint", order = 1)]
public class HingeJointBuilder : PhysJointBuilder
{
    public bool selfCollision;
    public bool isPowered;
    public Vector3 axis = new Vector3(1, 0, 0);
    
    public override Joint Build(Vector3 anchorPoint, Quaternion rot, PhysCluster originCluster, PhysCluster connectedCluster)
    {
        HingeJoint hingeJoint = originCluster.gameObject.AddComponent<HingeJoint>();
        hingeJoint.enableCollision = selfCollision;
        hingeJoint.anchor = anchorPoint;
        hingeJoint.connectedBody = connectedCluster.rb;
        hingeJoint.useMotor = isPowered;
        hingeJoint.axis = rot * axis;
        return hingeJoint;
    }
}
