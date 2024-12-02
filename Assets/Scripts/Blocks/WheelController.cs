using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

public class WheelController : MonoBehaviour
{
    [Header("References")]
    public ConstantForce wheelForce;
    public HingeJoint wheelJoint;
    public Rigidbody rb;
    
    float torqueDirection;
    float targetSteerRot;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        wheelJoint.connectedBody = Utils.FindParentRigidbody(transform, rb);
        
        torqueDirection = CalculateTorqueDirection();
    }

    public void SetForwardTorque( float torque)
    {
        JointMotor motor = wheelJoint.motor;

        motor.force = torque;
        
        wheelJoint.motor = motor;
        
        //wheelForce.relativeTorque = new Vector3(0,0, torque * torqueDirection);
    }
    
    public void SetTargetVelocity( float velocity)
    {

        JointMotor motor = wheelJoint.motor;

        motor.targetVelocity = velocity * torqueDirection;
        
        wheelJoint.motor = motor;
    }
    
    // Update is called once per frame
    void Update()
    {

    }

    
    float CalculateTorqueDirection()
    {
        Transform origin = transform.root;
        // Direction from a to b
        Vector3 directionToWheelBody = transform.position - origin.position;

        // Calculate the dot product with a's right direction
        float dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.right);

        // If the dot product is positive, b is on the right side of a
        return dotProduct > 0 ? 1 : -1;
        
    }
}
