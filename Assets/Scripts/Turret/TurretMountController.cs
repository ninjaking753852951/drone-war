using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityUtils;

public class TurretMountController : MonoBehaviour
{
    public Transform aimPoint;

    public HingeJoint yawJoint;
    public HingeJoint pitchJoint;

    public float readyToFirePositionThreshold = 5;
    public float readyToFireDelta = 0.1f;
    public float aimForce = 100;
    public float maxTurnSpeed = 10;
    public float nearTargetSmoothing = 1;
    
    TurretCoreController turret;
    
    float targetPitchAngle;
    float targetYawAngle;

    float pitchDelta;
    float pitchPos;

    float yawDelta;
    float yawPos;
    
    
    [HideInInspector]
    public Rigidbody yawRb;
    [HideInInspector]
    public Rigidbody pitchRb;

    Vector3 targetPosition;

    void Awake()
    {
        yawRb = yawJoint.GetComponent<Rigidbody>();
        yawRb.isKinematic = true;
        yawRb.useGravity = false;
        
        pitchRb = pitchJoint.GetComponent<Rigidbody>();
        pitchRb.isKinematic = true;
        pitchRb.useGravity = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        pitchDelta = pitchPos - pitchJoint.angle;
        pitchPos = pitchJoint.angle;

        yawDelta = yawPos - yawJoint.angle;
        yawPos = yawJoint.angle;
    }

    void Update()
    {
        SetPitchAngle(targetPitchAngle);
        SetYawAngle(targetYawAngle);
    }


    public void Deploy(TurretCoreController controller)
    {
        turret = controller;

        yawRb.isKinematic = false;
        yawRb.useGravity = true;
        
        pitchRb.isKinematic = false;
        pitchRb.useGravity = true;

        yawJoint.connectedBody = Utils.FindParentRigidbody(transform, yawRb);
        InitJointMotors();
    }

    public void UpdateTurretAim(TurretCoreController turret, Vector3 targetPos, Vector3 targetVelocity)
    {
        float interceptTime = EstimateInterceptTime(turret, targetPos, targetVelocity);
        Vector3 predictedTargetPos = targetPos + targetVelocity * interceptTime;

        UpdateYawAngle(turret, predictedTargetPos);
        
        targetPitchAngle = -turret.CalculateTargetPitchAngle(predictedTargetPos, interceptTime);
    }

    void InitPitchJoint()
    {
        JointMotor pitchMotor = pitchJoint.motor;
        pitchMotor.force = aimForce;
        pitchJoint.motor = pitchMotor;
        pitchJoint.useMotor = true;
    }
    
    private float EstimateInterceptTime(TurretCoreController turret, Vector3 targetPos, Vector3 targetVelocity)
    {
        float projectileVelocity = turret.shootVelocity;
        Vector3 turretPosition = aimPoint.position;

        float time = 0f;
        const float tolerance = 0.01f;
        const int maxIterations = 100;

        for (int i = 0; i < maxIterations; i++)
        {
            Vector3 predictedTargetPos = targetPos + targetVelocity * time;
            float horizontalDistance = (predictedTargetPos - turretPosition).magnitude;

            float newTime = this.turret.EstimateTimeOfFlight(projectileVelocity, horizontalDistance);
            if (Mathf.Abs(newTime - time) < tolerance)
                return newTime;

            time = newTime;
        }

        return time;
    }

    void UpdateYawAngle(TurretCoreController turret, Vector3 targetPos)
    {
        // Calculate the direction from the aimPoint to the target
        Vector3 directionToTarget = targetPos - aimPoint.position;
        
        // Create a horizontal direction vector by ignoring the Y component
        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);

        // Calculate the yaw angle (angle around the Y-axis)
        targetYawAngle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;

        // account for base rotation
        targetYawAngle = (targetYawAngle - aimPoint.rotation.eulerAngles.y) %360;

    }
    
    void SetPitchAngle(float targetPitchAngle)
    {
        float currentAngle = pitchJoint.angle;
        float angleError = targetPitchAngle - currentAngle;

        angleError = Mathf.Clamp(angleError/nearTargetSmoothing, -1, 1);
        
        JointMotor pitchMotor = pitchJoint.motor;
        pitchMotor.targetVelocity = angleError * maxTurnSpeed;
        pitchJoint.motor = pitchMotor;
    }
    
    void SetYawAngle(float targetYawAngle)
    {
        float currentAngle = (yawJoint.angle % 360 + 360) % 360;
        float normalizedTargetAngle = (targetYawAngle % 360 + 360) % 360;
        
        float angleError = Mathf.DeltaAngle(currentAngle, normalizedTargetAngle);
        
        angleError = Mathf.Clamp(angleError / nearTargetSmoothing, -1, 1);
        
        JointMotor yawMotor = yawJoint.motor;
        yawMotor.targetVelocity = angleError * maxTurnSpeed; // Adjust multiplier for speed control
        yawJoint.motor = yawMotor;
    }

    void InitJointMotors()
    {
        InitYawJoint();
        InitPitchJoint();
    }

    void InitYawJoint()
    {
        JointMotor yawMotor = yawJoint.motor;
        yawMotor.force = aimForce;
        yawJoint.motor = yawMotor;
        yawJoint.useMotor = true;
    }


    public bool ReadyToFire()
    {
        
        float maxAngle = GetMaxAllowableAngle(transform.position, targetPosition, readyToFirePositionThreshold);

        Vector2 curAngles = new Vector2(yawJoint.angle, pitchJoint.angle);
        Vector2 curTargetAngles = new Vector2(targetYawAngle, targetPitchAngle);

        Vector2 angleError = curAngles - curTargetAngles;
        angleError = new Vector2(angleError.x%360, angleError.y %360);
        Vector2 angleDelta = new Vector2(pitchDelta, yawDelta);

        return angleError.magnitude < maxAngle && angleDelta.magnitude < readyToFireDelta;
    }
    
    float GetMaxAllowableAngle(Vector3 P1, Vector3 P2, float tolerance)
    {
        float distance = Vector3.Distance(P1, P2);
        if (distance <= tolerance) return 180f; // Covers all directions
        return Mathf.Asin(tolerance / distance) * Mathf.Rad2Deg;
    }
}
