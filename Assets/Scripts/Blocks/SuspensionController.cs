using UnityEngine;

public class SuspensionController : MovingDroneBlockBase
{

    ConfigurableJoint joint;
    PhysJointPhysBlock block;
    
    public float springForce = 20000;
    
    public override void Deploy()
    {
        block = GetComponent<PhysJointPhysBlock>();
        joint = (ConfigurableJoint)block.joint;

        if(joint == null)
            return;
        
        JointDrive jointDrive = joint.yDrive;
        jointDrive.positionSpring = springForce;
        joint.yDrive = jointDrive;
    }

}
