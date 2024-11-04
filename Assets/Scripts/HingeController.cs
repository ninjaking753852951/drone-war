using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeController : MonoBehaviour
{
    public HingeJoint joint;
    public Rigidbody rb;

    public float turnLimit;
    public float maxTurnSpeed = 1000;
    
    float turnDirection;
    float targetSteer;
    
    // Start is called before the first frame update
    void Start()
    {

        
    }

    public void Init()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        
        joint.connectedBody = Utils.FindParentRigidbody(transform.parent, rb);
        turnDirection = CalculateTurnDirection();
    }

    // Update is called once per frame
    void Update()
    {
        JointSpring curSpring = joint.spring;
        curSpring.targetPosition = Mathf.MoveTowards(curSpring.targetPosition, targetSteer * turnDirection, Time.deltaTime * maxTurnSpeed);
        joint.spring = curSpring;
    }
    
    public void SetSteerRot(float steer)
    {
        targetSteer = Mathf.Clamp(steer, -turnLimit, turnLimit);
    }
    
    float CalculateTurnDirection()
    {
        
        
        Transform origin = transform.root;
        // Direction from a to b
        Vector3 directionToWheelBody = transform.position - origin.position;

        // Calculate the dot product with a's right direction
        float dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.forward);

        // If the dot product is positive, b is on the right side of a
        return dotProduct > 0 ? 1 : -1;
        
    }
}
