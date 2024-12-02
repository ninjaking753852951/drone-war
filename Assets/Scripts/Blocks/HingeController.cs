using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeController : MonoBehaviour
{
    public HingeJoint joint;
    public Rigidbody rb;

    public float turnLimit;
    public float maxTurnSpeed = 1000;

    public Transform body;

    [Header("Rollover Prevention")] 
    public float rollThreshold;

    public AnimationCurve turnSpeedCurve;
    public float minSpeedAttenuate;
    public float maxSpeedAttenuate;
    
    float turnDirection;
    float targetSteer;
    float targetSteerRollover;

    DroneController controller;

    bool isDeployed;
    
    // Start is called before the first frame update
    void Start()
    {

        
    }

    public void Init()
    {
        isDeployed = true;
        
        rb.isKinematic = false;
        rb.useGravity = true;

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
       
        
        float curRoll = transform.root.localRotation.eulerAngles.z;
        curRoll = (curRoll > 180) ? curRoll - 360 : curRoll;

        float curTurnSpeed = maxTurnSpeed *
                             turnSpeedCurve.Evaluate(1 - Mathf.InverseLerp(minSpeedAttenuate, maxSpeedAttenuate,
                                 controller.velocity));
        
        
        
        /*
        if (Mathf.Abs(curRoll) < rollThreshold)
        {
            targetSteerRollover = Mathf.MoveTowards(targetSteerRollover, targetSteer * turnDirection, Time.deltaTime * curTurnSpeed);
        }
        else
        {
            targetSteerRollover = Mathf.MoveTowards(targetSteerRollover, 0 , Time.deltaTime * maxTurnSpeed);
        }
        */
        
        targetSteerRollover = Mathf.MoveTowards(targetSteerRollover, targetSteer * turnDirection, Time.deltaTime * maxTurnSpeed);
        
        
        JointSpring curSpring = joint.spring;
        //curSpring.targetPosition = Mathf.MoveTowards(curSpring.targetPosition, targetSteerRollover * turnDirection, Time.deltaTime * maxTurnSpeed);
        curSpring.targetPosition = targetSteerRollover;
        joint.spring = curSpring;
    }
    
    public void SetSteerRot(float steer)
    {
        targetSteer = Mathf.Clamp(steer, -turnLimit, turnLimit);

        /*float safeTurnAngle = CalculateSafeTurnAngle(controller.velocity, controller.comHeight + 10);*/
        float safeTurnAngle = CalculateMaxSteeringAngle(controller.trackWidth, 9.81f, controller.wheelBase, controller.comHeight, controller.velocity);

        
        
        //Debug.Log(safeTurnAngle);
        
        
        
        //targetSteer = Mathf.Clamp(targetSteer, -safeTurnAngle, safeTurnAngle);
    }
    
    float CalculateMaxSteeringAngle(float trackWidth, float gravity, float wheelbase, float centerOfMassHeight, float speed)
    {
        float numerator = wheelbase * trackWidth * gravity;
        float denomenator = 2 * (centerOfMassHeight) * (speed * speed);

        //numerator *= 0.5f;
        // Calculate the turn angle in radians
        float turnAngleRadians = Mathf.Atan(numerator/ denomenator);

        // Convert the angle to degrees for easier interpretation (optional)
        float turnAngleDegrees = turnAngleRadians * Mathf.Rad2Deg;

        return turnAngleDegrees;
    }

    
    /*float CalculateMaxSteeringAngle(float speed, float centerOfMassHeight, float trackWidth, float wheelbase, float mass)
    {
        float gravity = 9.81f;

        // turn angle in radians
        float turnAngle = 10;
        
        // turn radius is wheelbase /tan (turn angle)
        float turnRadius = wheelbase/Mathf.Tan(turnAngle);
        
        // angular velocity is velocity/radius
        float angularVelocity = speed/turnRadius;
        
        // torque due to gravity half track width * mass * gravity
        float torqueGravity = (trackWidth / 2) * (mass * gravity);

        float centrifugalForce = mass * (angularVelocity * angularVelocity) * turnRadius;

        // torque due to centrifugal force 
        float torqueCentrifugal = centerOfMassHeight * centrifugalForce;

        (tW / 2) * m * g = comH * (m * (v / (wB /tan(a)))^2 * (wB / tan(a)))
        
        return turnAngle;
    }*/


    
    float CalculateTurnDirection()
    {
        
        
        Transform origin = transform.root;
        // Direction from a to b
        Vector3 directionToWheelBody = transform.position - origin.position;

        // Calculate the dot product with a's right direction
        float dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.right);

        // If the dot product is positive, b is on the right side of a
        float dir = dotProduct < 0 ? 1 : -1;
        
        dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.forward);
        
        return dotProduct > 0 ? dir * 1 :dir * -1;
        
    }
}
