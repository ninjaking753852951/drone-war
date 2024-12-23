using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeController : MovingDroneBlockBase, IProxyDeploy
{
    public HingeJoint joint;

    public float turnLimit = 45;
    public float maxTurnSpeed = 1000;

    public Transform body;

    float turnDirection;
    float targetSteer;
    float curSteer;

    DroneController controller;

    bool isDeployed;
    
    public override void Deploy()
    {
        base.Deploy();
        
        isDeployed = true;

        body.transform.parent = transform.parent;

        controller = transform.root.GetComponent<DroneController>();
        
        joint.connectedBody = Utils.FindParentRigidbody(transform.parent, rb);
        turnDirection = CalculateTurnDirection();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDeployed)
            return;
       

        curSteer = Mathf.MoveTowards(curSteer, targetSteer * turnDirection, Time.deltaTime * maxTurnSpeed);
        
        JointSpring curSpring = joint.spring;
        curSpring.targetPosition = curSteer;
        joint.spring = curSpring;
    }
    
    public void SetSteerRot(float steer)
    {
        float safeTurnAngle = controller.movementController.safeTurnAngle;

        targetSteer = steer;
        
        targetSteer = Mathf.Clamp(targetSteer, -turnLimit, turnLimit);
        
        targetSteer = Mathf.Clamp(targetSteer, -safeTurnAngle, safeTurnAngle);
    }
    
    float CalculateTurnDirection()
    {
        Transform origin = transform.root;
        // Direction from a to b
        Vector3 directionToWheelBody = transform.position - origin.position;

        // Calculate the dot product with a's right direction
        float dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.right);

        // If the dot product is positive, b is on the right side of a
        float dir = dotProduct < 0 ? -1 : -1;
        
        dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.forward);
        
        return dotProduct < 0 ? dir * -1 :dir * 1;
    }
    public void ProxyDeploy()
    {
        body.transform.parent = transform.parent;
    }
}
