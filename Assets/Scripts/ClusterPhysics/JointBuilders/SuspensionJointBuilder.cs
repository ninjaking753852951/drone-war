using UnityEngine;

[CreateAssetMenu(fileName = "SuspensionJoint", menuName = "ScriptableObjects/Physics/SuspensionJoint", order = 1)]
public class SuspensionJointBuilder : PhysJointBuilder
{
    public bool selfCollision;

    public float springValue;
    public float damperValue;
    
    public override Joint Build(Vector3 anchorPoint, PhysCluster originCluster, PhysCluster connectedCluster)
    {
        ConfigurableJoint suspensionJoint = originCluster.gameObject.AddComponent<ConfigurableJoint>();
        suspensionJoint.enableCollision = selfCollision;
        suspensionJoint.anchor = anchorPoint;
        suspensionJoint.connectedBody = connectedCluster.rb;
        
        // Lock all rotation
        suspensionJoint.angularXMotion = ConfigurableJointMotion.Locked;
        suspensionJoint.angularYMotion = ConfigurableJointMotion.Locked;
        suspensionJoint.angularZMotion = ConfigurableJointMotion.Locked;

        // Lock X and Z linear motion, allow only Y
        suspensionJoint.xMotion = ConfigurableJointMotion.Locked;
        suspensionJoint.yMotion = ConfigurableJointMotion.Free;  // or Free if you don't want limits
        suspensionJoint.zMotion = ConfigurableJointMotion.Locked;

        // Configure the spring
        var drive = new JointDrive
        {
            positionSpring = springValue,
            positionDamper = damperValue,
            maximumForce = float.MaxValue  // Allow full force
        };

        // Apply the drive settings to Y axis
        suspensionJoint.yDrive = drive;
        
        return suspensionJoint;
    }
}
