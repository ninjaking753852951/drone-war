using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretMountController : MonoBehaviour
{

    public Transform aimPoint;

    public HingeJoint yawJoint;
    public HingeJoint pitchJoint;

    public float readyToFireThreshold = 0.1f;
    
    float targetPitchAngle;
    float targetYawAngle;
    
    // Start is called before the first frame update
    void Start()
    {
        Deploy( false);
    }

    public void Deploy(bool deploy)
    {
        Rigidbody yawRb = yawJoint.GetComponent<Rigidbody>();
        yawRb.isKinematic = !deploy;
        yawRb.useGravity = deploy;
        
        Rigidbody pitch = pitchJoint.GetComponent<Rigidbody>();
        pitch.isKinematic = !deploy;
        pitch.useGravity = deploy;

        if (deploy)
        {
            yawJoint.connectedBody = Utils.FindParentRigidbody(transform, yawRb);
        }
    }

    public void UpdateTurretAim(TurretCoreController turret, Vector3 targetPos)
    {
        float projectileVelocity = turret.shootVelocity;
        float gravity = Physics.gravity.y; // Gravity acceleration, typically -9.81 m/s²
        
        // Calculate the direction from the aimPoint to the target
        Vector3 directionToTarget = targetPos - aimPoint.position;

        // ----------- Yaw Control (Horizontal movement) -----------

        // Create a horizontal direction vector by ignoring the Y component
        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);

        // Calculate the yaw angle (angle around the Y-axis)
        targetYawAngle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;

        // account for base rotation
        targetYawAngle = Mathf.Repeat(targetYawAngle - aimPoint.rotation.eulerAngles.y + 180, 360) - 180;
        
        // Set the yaw joint's spring target position
        JointSpring yawSpring = yawJoint.spring;
        yawSpring.targetPosition =  targetYawAngle; // Set the target yaw angle
        yawJoint.spring = yawSpring;

        // ----------- Pitch Control (Vertical movement with bullet drop) -----------

        // Calculate the horizontal distance in the XZ plane
        float horizontalDistance = horizontalDirection.magnitude;

        // Use the horizontal distance and projectile velocity to calculate the time of flight
        float timeOfFlight = horizontalDistance / projectileVelocity;

        // Calculate the vertical displacement due to gravity over the time of flight
        // Formula: y = 0.5 * g * t^2 (since initial vertical velocity is 0 for aiming)
        float verticalDrop = 0.5f * Mathf.Abs(gravity) * Mathf.Pow(timeOfFlight, 2);

        // Adjust the direction to target for the vertical drop
        float adjustedTargetY = directionToTarget.y + verticalDrop;

        // Calculate the adjusted pitch angle based on the new vertical difference and horizontal distance
        targetPitchAngle = Mathf.Atan2(adjustedTargetY, horizontalDistance) * Mathf.Rad2Deg * -1;

        // Set the pitch joint's spring target position
        JointSpring pitchSpring = pitchJoint.spring;
        pitchSpring.targetPosition = targetPitchAngle; // Set the target pitch angle
        pitchJoint.spring = pitchSpring;
    }

    public bool ReadyToFire()
    {
        JointSpring pitchSpring = pitchJoint.spring;
        JointSpring yawSpring = yawJoint.spring;

        Vector2 curAngles = new Vector2(yawJoint.angle, pitchJoint.angle);

        Vector2 curTargetAngles = new Vector2(targetYawAngle, targetPitchAngle);

        Vector2 angleDelta = curAngles - curTargetAngles;

        Debug.Log(angleDelta.magnitude < readyToFireThreshold);
        
        return angleDelta.magnitude < readyToFireThreshold;
    }


}
