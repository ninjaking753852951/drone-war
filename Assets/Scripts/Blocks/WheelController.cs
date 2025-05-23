using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

public class WheelController : MovingDroneBlockBase
{
    [Header("References")]
    public HingeJoint wheelJoint;
    
    public float torqueDirection;
    float targetSteerRot;

    public PhysJointPhysBlock block;

    DroneController controller;
    
    public override void Deploy()
    {
        //Debug.Log("Wheel deploy");
        controller = transform.root.GetComponentInChildren<DroneController>();
        
        block = GetComponent<PhysJointPhysBlock>();
        wheelJoint = (HingeJoint)block.joint;
        
        torqueDirection = CalculateTorqueDirection();
    }

    public void SetForwardTorque(float torque)
    {
        JointMotor motor = wheelJoint.motor;

        motor.force = torque;
        
        wheelJoint.motor = motor;
    }
    
    public void SetTargetVelocity(float velocity)
    {
        JointMotor motor = wheelJoint.motor;

        motor.targetVelocity = velocity * torqueDirection;
        
        wheelJoint.motor = motor;
    }
    
    float CalculateTorqueDirection()
    {
        
        Transform origin = controller.transform;
        // Direction from a to b
        Vector3 directionToWheelBody = transform.position - origin.position;
        
        
        // Calculate the dot product with a's right direction
        float dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.right);

        //return 1;
        //Debug.Log(dotProduct);
        
        // If the dot product is positive, b is on the right side of a
        return dotProduct > 0 ? 1 : -1;
    }
}
